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
    sealed class CompoundSyntax : Syntax
    {
        [UsedImplicitly]
        internal static bool IsInContainerDump;

        static readonly string RunId = Extension.GetFormattedNow() + "\n";
        static bool IsInsideFileDump;
        static int NextObjectId;

        readonly Syntax CleanupSection;
        readonly Statement[] StatementsData;

        CompoundSyntax(Statement[] rawStatements, Syntax cleanupSection, BinaryTree binaryTree)
            : base(NextObjectId++, binaryTree)
        {
            StatementsData = rawStatements;
            CleanupSection = cleanupSection;
        }

        [DisableDump]
        public IEnumerable<FunctionSyntax> ConverterFunctions
            => StatementsData
                .Where(data => data.IsConverterSyntax)
                .Select(data => (FunctionSyntax)data.Syntax);

        [Node]
        [EnableDump]
        internal Syntax[] Statements => StatementsData.Select(s => s.Syntax).ToArray();

        [EnableDump]
        internal IDictionary<string, int> NameIndex
            => StatementsData
                .SelectMany
                (
                    (statement, index) => statement.AllNames.Select
                    (
                        name => new
                        {
                            Key = name, Value = index
                        }
                    )
                )
                .ToDictionary(item => item.Key, item => item.Value);

        [EnableDump]
        internal int[] Mutables => IndexList(item => item.IsMutableSyntax).ToArray();

        [EnableDump]
        internal int[] Converters => IndexList(item => item.IsConverterSyntax).ToArray();

        [EnableDump]
        internal int[] MixIns => IndexList(item => item.IsMixInSyntax).ToArray();

        [DisableDump]
        internal int EndPosition => Statements.Length;


        [DisableDump]
        internal override bool? IsHollow => Statements.All(syntax => syntax.IsHollow == true);

        [DisableDump]
        internal Size IndexSize => Size.AutoSize(Statements.Length);

        [DisableDump]
        internal string[] AllNames => StatementsData.SelectMany(s => s.AllNames).ToArray();

        [DisableDump]
        internal int[] ConverterStatementPositions
            => StatementsData
                .SelectMany((s, i) => s.IsConverterSyntax? new[] {i} : new int[0])
                .ToArray();

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
            => T(StatementsData.Select(s => s.GetChildren()), T(CleanupSection)).Concat();

        internal static Result<Syntax> Create(Result<Statement> statement, BinaryTree binaryTree)
            => new Result<Syntax>(new CompoundSyntax(new[] {statement.Target}, null, binaryTree), statement.Issues);

        internal static Result<Syntax> Create(Result<Statement[]> statements, BinaryTree binaryTree)
            => new Result<Syntax>(new CompoundSyntax(statements.Target, null, binaryTree), statements.Issues);

        internal static Result<Syntax> Create
            (Result<Statement[]> statements, Result<Syntax> cleanup, BinaryTree binaryTree)
            => new Result<Syntax>
            (
                new CompoundSyntax(statements.Target, cleanup?.Target, binaryTree),
                statements.Issues.plus(cleanup?.Issues)
            );

        internal bool IsMutable(int position) => StatementsData[position].IsMutableSyntax;

        internal int? Find(string name, bool publicOnly)
        {
            if(name == null)
                return null;

            return StatementsData
                .Select((data, index) => data.IsDefining(name, publicOnly)? index : (int?)null)
                .FirstOrDefault();
        }

        internal override Result ResultForCache(ContextBase context, Category category)
        {
            var compound = context.Compound(this);
            if(compound.HasIssue)
                return Feature.Extension.Result(compound.Issues, category);
            return compound.Result(category);
        }

        internal override Syntax Visit(ISyntaxVisitor visitor)
        {
            var statements = StatementsData.Select(s => s.Visit(visitor)).ToArray();
            var cleanupSection = CleanupSection?.Visit(visitor);

            if(statements.All(s => s == null) && cleanupSection == null)
                return null;

            var newStatements = statements.Select((s, i) => s ?? StatementsData[i]).ToArray();
            var newCleanupSection = cleanupSection ?? CleanupSection;
            return new CompoundSyntax(newStatements, newCleanupSection, BinaryTree);
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

        IEnumerable<int> IndexList(Func<Statement, bool> selector)
        {
            for(var index = 0; index < StatementsData.Length; index++)
                if(selector(StatementsData[index]))
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