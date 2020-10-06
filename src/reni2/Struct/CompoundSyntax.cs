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
        [UsedImplicitly]
        internal static bool IsInContainerDump;

        static readonly string RunId = Extension.GetFormattedNow() + "\n";
        static bool IsInsideFileDump;
        static int NextObjectId;

        readonly ValueSyntax CleanupSection;
        readonly DeclarationSyntax[] Statements;

        CompoundSyntax(DeclarationSyntax[] statements, ValueSyntax cleanupSection, BinaryTree target)
            : base(NextObjectId++, target)
        {
            Statements = statements;
            CleanupSection = cleanupSection;
        }

        CompoundSyntax(Statement[] statements, ValueSyntax cleanupSection, BinaryTree target)
            : base(NextObjectId++, target)
            => NotImplementedMethod(statements, cleanupSection, target);

        [DisableDump]
        public IEnumerable<FunctionSyntax> ConverterFunctions
            => Statements
                .Where(data => data.IsConverterSyntax)
                .Select(data => (FunctionSyntax)data.Value);

        [Node]
        [EnableDump]
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
            .Where(name=>name!= null)
            .ToArray();

        [DisableDump]
        internal int[] ConverterStatementPositions
            => Statements
                .SelectMany((s, i) => s.IsConverterSyntax? new[] {i} : new int[0])
                .ToArray();

        internal static Result<ValueSyntax> Create(DeclarationSyntax statement, BinaryTree root)
            => new CompoundSyntax(T(statement), null, root);

        internal static Result<ValueSyntax> Create(Result<Statement> statement, BinaryTree binaryTree)
            => new Result<ValueSyntax>(new CompoundSyntax(new[] {statement.Target}, null, binaryTree)
                , statement.Issues);

        internal static Result<ValueSyntax> Create(Result<Statement[]> statements, BinaryTree binaryTree)
            => new Result<ValueSyntax>(new CompoundSyntax(statements.Target, null, binaryTree), statements.Issues);

        internal static Result<ValueSyntax> Create
            (Result<Statement[]> statements, Result<ValueSyntax> cleanup, BinaryTree binaryTree)
            => new Result<ValueSyntax>
            (
                new CompoundSyntax(statements.Target, cleanup?.Target, binaryTree),
                statements.Issues.plus(cleanup?.Issues)
            );

        public string GetCompoundIdentificationDump() => "." + ObjectId + "i";

        public override string DumpData()
        {
            var isInsideFileDump = IsInsideFileDump;
            IsInsideFileDump = true;
            var result = isInsideFileDump? DumpDataToString() : DumpDataToFile();
            IsInsideFileDump = isInsideFileDump;
            return result;
        }

        protected override string GetNodeDump()
            => GetType().PrettyName() + "(" + GetCompoundIdentificationDump() + ")";

        protected override IEnumerable<Syntax> GetChildren()
            => T(Statements.Cast<Syntax>(), T(CleanupSection)).Concat();

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
            return new CompoundSyntax(newStatements, newCleanupSection, Target);
        }

        internal Result Cleanup(ContextBase context, Category category)
        {
            if(CleanupSection != null && (category.HasCode || category.HasExts))
                return context
                    .Result(category.Typed, CleanupSection)
                    .Conversion(context.RootContext.VoidType)
                    .LocalBlock(category) & category;

            return context.RootContext.VoidType.Result(category);
        }

        IEnumerable<int> IndexList(Func<DeclarationSyntax, bool> selector)
        {
            for(var index = 0; index < Statements.Length; index++)
                if(selector(Statements[index]))
                    yield return index;
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