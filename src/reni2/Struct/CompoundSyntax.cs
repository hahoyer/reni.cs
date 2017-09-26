using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
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
        internal static Result<Value> Create(Result<Statement> item, Syntax syntax)
            => new Result<Value>(new CompoundSyntax(new[] {item.Target}, syntax), item.Issues);

        internal static Result<Value> Create(Result<Statement[]> item, Syntax syntax)
            => new Result<Value>(new CompoundSyntax(item.Target, syntax), item.Issues);

        internal static Result<Value> Create
            (Result<Statement[]> left, Result<Value> right, Syntax syntax)
            => new Result<Value>
            (
                new CompoundSyntax(left.Target, syntax, right?.Target),
                left.Issues.plus(right?.Issues)
            );

        readonly Statement[] _statements;
        readonly Data[] _data;
        readonly Value CleanupSection;
        static readonly string _runId = Extension.GetFormattedNow() + "\n";
        internal static bool IsInContainerDump;
        static bool _isInsideFileDump;
        static int _nextObjectId;

        CompoundSyntax(Statement[] statements, Syntax syntax, Value cleanupSection = null)
            : base(_nextObjectId++, syntax)
        {
            _statements = statements;
            _data = GetData;
            CleanupSection = cleanupSection;
        }

        public string GetCompoundIdentificationDump() => "." + ObjectId + "i";
        [Node]
        [EnableDump]
        internal Value[] Statements => _data.Select(s => s.Statement).ToArray();

        [EnableDump]
        internal IDictionary<string, int> NameIndex
            => _data
                .SelectMany
                (
                    (statement, index) => statement.AllNames.Select
                    (
                        name => new
                        {
                            Key = name,
                            Value = index
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

        IEnumerable<int> IndexList(Func<Data, bool> selector)
        {
            for(var index = 0; index < _data.Length; index++)
                if(selector(_data[index]))
                    yield return index;
        }

        [DisableDump]
        internal int EndPosition => Statements.Length;


        [DisableDump]
        internal override bool? Hllw => Statements.All(syntax => syntax.Hllw == true);
        [DisableDump]
        internal Size IndexSize => Size.AutoSize(Statements.Length);

        [DisableDump]
        internal string[] AllNames => _data.SelectMany(s => s.AllNames).ToArray();

        [DisableDump]
        internal int[] ConverterStatementPositions
            => _data
                .SelectMany((s, i) => s.IsConverter ? new[] {i} : new int[0])
                .ToArray();

        [DisableDump]
        public IEnumerable<FunctionSyntax> ConverterFunctions
            => _data
                .Where(data => data.IsConverter)
                .Select(data => (FunctionSyntax) data.Statement);

        [DisableDump]
        Data[] GetData
            => _statements
                .Select((s, i) => new Data(s, i))
                .ToArray();

        protected override string GetNodeDump()
            => GetType().PrettyName() + "(" + GetCompoundIdentificationDump() + ")";

        internal bool IsMutable(int position) => _data[position].IsMutable;

        public override string DumpData()
        {
            var isInsideFileDump = _isInsideFileDump;
            _isInsideFileDump = true;
            var result = isInsideFileDump ? DumpDataToString() : DumpDataToFile();
            _isInsideFileDump = isInsideFileDump;
            return result;
        }

        string DumpDataToFile()
        {
            var dumpFile = ("compound." + ObjectId).ToSmbFile();
            var oldResult = dumpFile.String;
            var newResult = (_runId + DumpDataToString()).Replace("\n", "\r\n");
            if(oldResult == null || !oldResult.StartsWith(_runId))
            {
                oldResult = newResult;
                dumpFile.String = oldResult;
            }
            else
                Tracer.Assert(oldResult == newResult);
            return Tracer.FilePosn(dumpFile.FullName, 1, 0, FilePositionTag.Debug) + "see there\n";
        }

        string DumpDataToString()
        {
            var isInDump = IsInContainerDump;
            IsInContainerDump = true;
            var result = base.DumpData();
            IsInContainerDump = isInDump;
            return result;
        }

        internal int? Find(string name, bool publicOnly)
        {
            if(name == null)
                return null;

            return _data
                .SingleOrDefault(s => s.IsDefining(name, publicOnly))
                ?.Position;
        }

        internal override Result ResultForCache(ContextBase context, Category category)
            => context
                .Compound(this)
                .Result(category);

        sealed class Data : DumpableObject
        {
            public Data(Statement rawStatement, int position)
            {
                RawStatement = rawStatement;
                Position = position;
                StatementCache = new ValueCache<Result<Value>>(GetStatement);
                AllNamesCache = new ValueCache<string[]>(GetAllNames);
                PublicNamesCache = new ValueCache<string[]>(GetPublicNames);
                Tracer.Assert(RawStatement != null);
            }

            Statement RawStatement { get; }
            public int Position { get; }

            ValueCache<string[]> AllNamesCache { get; }
            ValueCache<string[]> PublicNamesCache { get; }
            ValueCache<Result<Value>> StatementCache { get; }

            public Value Statement => StatementCache.Value.Target;
            public Issue[] Issues => StatementCache.Value.Issues;

            public bool IsDefining(string name, bool publicOnly)
                => (publicOnly ? PublicNames : AllNames)
                    .Contains(name);

            public bool IsConverter => RawStatement.IsConverterSyntax;
            public bool IsMixIn => RawStatement.IsMixInSyntax;
            public IEnumerable<string> AllNames => AllNamesCache.Value;
            public IEnumerable<string> PublicNames => PublicNamesCache.Value;
            public bool IsMutable => RawStatement.IsMutableSyntax;

            Result<Value> GetStatement() => RawStatement.Body;
            string[] GetAllNames() => RawStatement.GetAllDeclarations().ToArray();
            string[] GetPublicNames() => RawStatement.GetPublicDeclarations().ToArray();
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
    }
}