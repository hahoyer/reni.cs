using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Forms;
using hw.Helper;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Parser;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.Struct
{
    /// <summary>
    ///     Structured data, context free version
    /// </summary>
    sealed class CompoundSyntax : CompileSyntax
    {
        readonly Syntax[] _statements;
        readonly Data[] _data;
        static readonly string _runId = Compiler.FormattedNow + "\n";
        internal static bool IsInContainerDump;
        static bool _isInsideFileDump;
        static int _nextObjectId;

        internal CompoundSyntax(Syntax[] statements)
            : base(_nextObjectId++)
        {
            _statements = statements;
            _data = GetData;
            StopByObjectIds();
        }

        public string GetCompoundIdentificationDump() => "." + ObjectId + "i";
        [Node]
        [EnableDump]
        internal CompileSyntax[] Statements => _data.Select(s => s.Statement).ToArray();

        [EnableDump]
        internal IDictionary<string, int> NameIndex
            => _data
                .SelectMany
                (
                    (statement, index) => statement.Names.Select
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
        internal override Checked<CompileSyntax> ToCompiledSyntax => this;

        internal override ResultCache.IResultProvider FindSource
            (IContextReference ext, ContextBase context)
        {
            var result = _data
                .SelectMany(item => item.Statement.ResultCache)
                .Where(item => item.Value.Exts.Contains(ext))
                .Where(item => (item.Key as CompoundContext)?.View.Compound.Syntax == this)
                .Where(item => (item.Key as Child)?.Parent == context)
                ;

            return result.FirstOrDefault().Value?.Provider;
        }


        [DisableDump]
        internal override bool? Hllw => Statements.All(syntax => syntax.Hllw == true);
        [DisableDump]
        internal Size IndexSize => Size.AutoSize(Statements.Length);

        [DisableDump]
        internal string[] Names => _data.SelectMany(s => s.Names).ToArray();

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

        internal override Checked<CompoundSyntax> ToCompound => this;

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
            var dumpFile = ("compound." + ObjectId).FileHandle();
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

        internal int? Find(string name)
        {
            if(name == null)
                return null;
            return _data
                .SingleOrDefault(s => s.IsDefining(name))
                ?.Position;
        }

        internal override Result ResultForCache(ContextBase context, Category category) 
            => context
            .Compound(this)
            .Result(category);

        sealed class Data : DumpableObject
        {
            public Data(Syntax rawStatement, int position)
            {
                RawStatement = rawStatement;
                Position = position;
                StatementCache = new ValueCache<Checked<CompileSyntax>>(GetStatement);
                NamesCache = new ValueCache<string[]>(GetNames);
                Tracer.Assert(RawStatement != null);
            }

            internal Syntax RawStatement { get; }
            public int Position { get; }

            ValueCache<string[]> NamesCache { get; }
            ValueCache<Checked<CompileSyntax>> StatementCache { get; }

            public CompileSyntax Statement => StatementCache.Value.Value;
            public Issue[] Issues => StatementCache.Value.Issues;
            public bool IsDefining(string name) => Names.Contains(name);
            public bool IsConverter => RawStatement.IsConverterSyntax;
            public bool IsMixIn => RawStatement.IsMixInSyntax;
            public IEnumerable<string> Names => NamesCache.Value;

            public bool IsMutable => RawStatement.IsMutableSyntax;

            Checked<CompileSyntax> GetStatement()
                => RawStatement.ContainerStatementToCompileSyntax;

            string[] GetNames() => RawStatement.GetDeclarations().ToArray();
        }

        [DisableDump]
        protected override IEnumerable<Syntax> DirectChildren => _statements;
    }
}