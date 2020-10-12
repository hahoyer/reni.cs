using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using JetBrains.Annotations;
using Reni.Basics;
using Reni.Context;
using Reni.Parser;
using Reni.TokenClasses;

namespace Reni.Struct
{
    /// <summary>
    ///     Structured data, context free version
    /// </summary>
    sealed class CompoundSyntax : ValueSyntax
    {
        static bool NoFileDump = true;
        static bool IsInContainerDump;

        static readonly string RunId = Extension.GetFormattedNow() + "\n";
        static bool IsInsideFileDump;
        static int NextObjectId;

        [EnableDump]
        internal readonly ValueSyntax CleanupSection;

        [EnableDump]
        internal readonly DeclarationSyntax[] Statements;

        internal CompoundSyntax(DeclarationSyntax[] statements, ValueSyntax cleanupSection, BinaryTree target)
            : base(NextObjectId++, target)
        {
            Statements = statements;
            CleanupSection = cleanupSection;

            AssertValid();
        }

        [DisableDump]
        public IEnumerable<FunctionSyntax> ConverterFunctions
            => Statements
                .Where(data => data.IsConverterSyntax)
                .Select(data => (FunctionSyntax)data.Value);

        [DisableDump]
        internal ValueSyntax[] PureStatements => Statements.Select(s => s.Value).ToArray();

        [EnableDump]
        internal IDictionary<string, int> NameIndex
            => Statements
                .Select((statement, index) => (Key: statement.NameOrNull, Value: index))
                .Where(pair => pair.Key != null)
                .ToDictionary(item => item.Key, item => item.Value);

        [EnableDump]
        internal int[] MutableDeclarations => IndexList(item => item.IsMutableSyntax).ToArray();

        [EnableDump]
        internal int[] Converters => IndexList(item => item.IsConverterSyntax).ToArray();

        [EnableDump]
        internal int[] MixInDeclarations => IndexList(item => item.IsMixInSyntax).ToArray();

        [DisableDump]
        internal int EndPosition => PureStatements.Length;


        [DisableDump]
        internal override bool? IsHollow => PureStatements.All(syntax => syntax.IsHollow == true);

        [DisableDump]
        internal Size IndexSize => Size.AutoSize(PureStatements.Length);

        [DisableDump]
        internal string[] AllNames => Statements
            .Select(s => s.NameOrNull)
            .Where(name => name != null)
            .ToArray();

        [DisableDump]
        internal int[] ConverterStatementPositions
            => Statements
                .SelectMany((s, i) => s.IsConverterSyntax? new[] {i} : new int[0])
                .ToArray();


        
        [DisableDump]
        protected override int LeftChildCount => Statements.Length;

        [DisableDump]
        protected override int DirectChildCount => Statements.Length + 1;

        public string GetCompoundIdentificationDump() => "." + ObjectId + "i";

        public override string DumpData()
        {
            var isInsideFileDump = IsInsideFileDump;
            IsInsideFileDump = true;
            var result = NoFileDump || isInsideFileDump? DumpDataToString() : DumpDataToFile();
            IsInsideFileDump = isInsideFileDump;
            return result;
        }

        protected override string GetNodeDump()
            => GetType().PrettyName() + "(" + GetCompoundIdentificationDump() + ")";

        protected override Syntax GetDirectChild(int index)
        {
            if(index >= 0 && index < Statements.Length)
                return Statements[index];
            return index == Statements.Length? CleanupSection : null;
        }

        internal bool IsMutable(int position) => Statements[position].IsMutableSyntax;

        internal int? Find(string name, bool publicOnly)
        {
            if(name == null)
                return null;

            return Statements
                .Select((data, index) => data.IsDefining(name, publicOnly)? index : (int?)null)
                .FirstOrDefault(data => data != null);
        }

        internal override Result ResultForCache(ContextBase context, Category category)
        {
            var compound = context.Compound(this);
            if(compound.HasIssue)
                return Feature.Extension.Result(compound.Issues, category);
            return compound.Result(category);
        }

        internal override ValueSyntax Visit(ISyntaxVisitor visitor)
        {
            var statements = Statements.Select(s => s.Visit(visitor)).ToArray();
            var cleanupSection = CleanupSection?.Visit(visitor);

            if(statements.All(s => s == null) && cleanupSection == null)
                return null;

            var newStatements = statements
                .Select((s, i) => s ?? Statements[i])
                .ToArray();
            var newCleanupSection = cleanupSection ?? CleanupSection;
            return new CompoundSyntax(newStatements, newCleanupSection, Binary);
        }

        internal Result Cleanup(ContextBase context, Category category)
        {
            if(CleanupSection != null && (category.HasCode || category.HasClosures))
                return context
                    .Result(category.WithType, CleanupSection)
                    .Conversion(context.RootContext.VoidType)
                    .LocalBlock(category) & category;

            return context.RootContext.VoidType.Result(category);
        }

        IEnumerable<int> IndexList(Func<DeclarerSyntax, bool> selector)
        {
            for(var index = 0; index < Statements.Length; index++)
            {
                var declarer = Statements[index].Declarer;
                if(declarer != null && selector(declarer))
                    yield return index;
            }
        }

        string DumpDataToFile()
        {
            var dumpFile = ("compound." + ObjectId).ToSmbFile();
            var oldResult = dumpFile.String;
            var newResult = (RunId + DumpDataToString()).Replace("\n", "\r\n");
            if(oldResult == null || !oldResult.StartsWith(RunId))
            {
                oldResult = newResult;
                dumpFile.String = oldResult;
            }
            else
                Tracer.Assert(oldResult == newResult);

            return Tracer.FilePosition(dumpFile.FullName, 1, 0, FilePositionTag.Debug) + "see there\n";
        }

        string DumpDataToString()
        {
            var isInDump = IsInContainerDump;
            IsInContainerDump = true;
            var result = base.DumpData();
            IsInContainerDump = isInDump;
            return result;
        }
    }
}