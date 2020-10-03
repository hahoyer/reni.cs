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
using Reni.Validation;

namespace Reni.Struct
{
    /// <summary>
    ///     Structured data, context free version
    /// </summary>
    sealed class CompoundSyntax : Value
    {
        sealed class StatementData : DumpableObject
        {
            class CacheContainer
            {
                public ValueCache<string[]> AllNames;
                public ValueCache<string[]> PublicNames;
                public ValueCache<Result<Value>> Syntax;
            }

            internal int Position { get; }

            internal Statement RawStatement { get; }

            readonly CacheContainer Cache = new CacheContainer();

            public StatementData(Statement rawStatement, int position)
            {
                RawStatement = rawStatement;
                Position = position;
                Cache.Syntax = new ValueCache<Result<Value>>(GetSyntax);
                Cache.AllNames = new ValueCache<string[]>(GetAllNames);
                Cache.PublicNames = new ValueCache<string[]>(GetPublicNames);
                Tracer.Assert(RawStatement != null);
            }

            public Value Value => Cache.Syntax.Value.Target;
            public Issue[] Issues => Cache.Syntax.Value.Issues;

            public bool IsConverter => RawStatement.IsConverterSyntax;
            public bool IsMixIn => RawStatement.IsMixInSyntax;
            public IEnumerable<string> AllNames => Cache.AllNames.Value;
            public IEnumerable<string> PublicNames => Cache.PublicNames.Value;
            public bool IsMutable => RawStatement.IsMutableSyntax;

            public bool IsDefining(string name, bool publicOnly)
                => (publicOnly? PublicNames : AllNames)
                    .Contains(name);

            internal Value GetChildren() => RawStatement.GetChildren();

            internal Statement Visit(ISyntaxVisitor visitor) => RawStatement.Visit(visitor);

            Result<Value> GetSyntax() => RawStatement.Body;
            string[] GetAllNames() => RawStatement.GetAllDeclarations().ToArray();
            string[] GetPublicNames() => RawStatement.GetPublicDeclarations().ToArray();
        }

        [UsedImplicitly]
        internal static bool IsInContainerDump;

        static readonly string RunId = Extension.GetFormattedNow() + "\n";
        static bool IsInsideFileDump;
        static int NextObjectId;

        readonly Value CleanupSection;
        readonly StatementData[] StatementsData;

        CompoundSyntax(Statement[] rawStatements, Value cleanupSection, BinaryTree binaryTree)
            : base(NextObjectId++, binaryTree)
        {
            StatementsData = GetData(rawStatements);
            CleanupSection = cleanupSection;
        }

        [DisableDump]
        public IEnumerable<FunctionSyntax> ConverterFunctions
            => StatementsData
                .Where(data => data.IsConverter)
                .Select(data => (FunctionSyntax)data.Value);

        [Node]
        [EnableDump]
        internal Value[] Statements => StatementsData.Select(s => s.Value).ToArray();

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
        internal int[] Mutables => IndexList(item => item.IsMutable).ToArray();

        [EnableDump]
        internal int[] Converters => IndexList(item => item.IsConverter).ToArray();

        [EnableDump]
        internal int[] MixIns => IndexList(item => item.IsMixIn).ToArray();

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
                .SelectMany((s, i) => s.IsConverter? new[] {i} : new int[0])
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

        protected override IEnumerable<Value> GetChildren()
            => T(StatementsData.Select(s => s.GetChildren()), T(CleanupSection)).Concat();

        internal static Result<Value> Create(Result<Statement> statement, BinaryTree binaryTree)
            => new Result<Value>(new CompoundSyntax(new[] {statement.Target}, null, binaryTree), statement.Issues);

        internal static Result<Value> Create(Result<Statement[]> statements, BinaryTree binaryTree)
            => new Result<Value>(new CompoundSyntax(statements.Target, null, binaryTree), statements.Issues);

        internal static Result<Value> Create(Result<Statement[]> statements, Result<Value> cleanup, BinaryTree binaryTree)
            => new Result<Value>
            (
                new CompoundSyntax(statements.Target, cleanup?.Target, binaryTree),
                statements.Issues.plus(cleanup?.Issues)
            );

        internal bool IsMutable(int position) => StatementsData[position].IsMutable;

        internal int? Find(string name, bool publicOnly)
        {
            if(name == null)
                return null;

            return StatementsData
                .SingleOrDefault(s => s.IsDefining(name, publicOnly))
                ?.Position;
        }

        internal override Result ResultForCache(ContextBase context, Category category)
        {
            var compound = context.Compound(this);
            if(compound.HasIssue)
                return Feature.Extension.Result(compound.Issues, category);
            return compound.Result(category);
        }

        internal override Value Visit(ISyntaxVisitor visitor)
        {
            var statements = StatementsData.Select(s => s.RawStatement.Visit(visitor)).ToArray();
            var cleanupSection = CleanupSection?.Visit(visitor);

            if(statements.All(s => s == null) && cleanupSection == null)
                return null;

            var newStatements = statements.Select((s, i) => s ?? StatementsData[i].RawStatement).ToArray();
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

        static StatementData[] GetData(Statement[] rawStatements)
            => rawStatements
                .Select((statement, index) => new StatementData(statement, index))
                .ToArray();

        IEnumerable<int> IndexList(Func<StatementData, bool> selector)
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

    #region

    #endregion
}