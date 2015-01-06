using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using hw.Debug;
using hw.Forms;
using hw.Parser;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.ReniParser;
using Reni.ReniSyntax;

namespace Reni.Struct
{
    /// <summary>
    ///     Structured data, context free version
    /// </summary>
    sealed class CompoundSyntax : CompileSyntax
    {
        readonly Data[] _data;
        static readonly string _runId = Compiler.FormattedNow + "\n";
        internal static bool IsInContainerDump;
        static bool _isInsideFileDump;
        static int _nextObjectId;

        internal CompoundSyntax(SourcePart token, Syntax[] statements)
            : base(token, _nextObjectId++)
        {
            _data = statements
                .Select((s, i) => new Data(s, i))
                .ToArray();
        }

        [Node]
        internal CompileSyntax[] Statements => _data.Select(s => s.Statement).ToArray();
        [DisableDump]
        internal int EndPosition => Statements.Length;
        [DisableDump]
        internal override CompileSyntax ToCompiledSyntax => this;
        [DisableDump]
        internal override bool? Hllw => Statements.All(syntax => syntax.Hllw == true);
        [DisableDump]
        internal Size IndexSize => Size.AutoSize(Statements.Length);
        [DisableDump]
        protected override ParsedSyntax[] Children => Statements.ToArray<ParsedSyntax>();
        [DisableDump]
        internal string[] Names { get { return _data.SelectMany(s => s.Names).ToArray(); } }
        [DisableDump]
        internal int[] Converters { get { return _data.SelectMany((s, i) => s.IsConverter ? new[] {i} : new int[0]).ToArray(); } }

        protected override string GetNodeDump() => "Compound." + ObjectId;
        internal bool IsReassignable(int position) => _data[position].IsReassignable;

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
            var dumpFile = ("struct." + ObjectId).FileHandle();
            var oldResult = dumpFile.String;
            var newResult = (_runId + DumpDataToString()).Replace("\n", "\r\n");
            if(oldResult == null || !oldResult.StartsWith(_runId))
            {
                oldResult = newResult;
                dumpFile.String = oldResult;
            }
            else
                Tracer.Assert(oldResult == newResult);
            return Tracer.FilePosn(dumpFile.FullName, 1, 0, FilePositionTag.Debug) + "see there" + "\n";
        }

        string DumpDataToString()
        {
            var isInDump = IsInContainerDump;
            IsInContainerDump = true;
            var result = base.DumpData();
            IsInContainerDump = isInDump;
            return result;
        }

        internal Position Find(string name)
        {
            if(name == null)
                return null;
            var result = _data.SingleOrDefault(s => s.Defines(name));
            if(result == null)
                return null;

            return new Position(result.Position);
        }

        internal override Result ObtainResult(ContextBase context, Category category) => context
            .UniqueCompound(this)
            .Result(category);

        sealed class Data : DumpableObject
        {
            public Data(Syntax rawStatement, int position)
            {
                RawStatement = rawStatement;
                Position = position;
                StatementCache = new ValueCache<CompileSyntax>(GetStatement);
                NamesCache = new ValueCache<string[]>(GetNames);
            }

            Syntax RawStatement { get; }
            public int Position { get; }

            ValueCache<string[]> NamesCache { get; }
            ValueCache<CompileSyntax> StatementCache { get; }

            public CompileSyntax Statement => StatementCache.Value;
            public bool Defines(string name) => Names.Contains(name);
            public bool IsConverter => RawStatement is ConverterSyntax;
            public IEnumerable<string> Names => NamesCache.Value;
            public bool IsReassignable => RawStatement.IsEnableReassignSyntax;

            CompileSyntax GetStatement() => RawStatement.ContainerStatementToCompileSyntax;
            string[] GetNames() => RawStatement.GetDeclarations().ToArray();
        }

        internal sealed class Position : DumpableObject
        {
            internal int Value { get; }
            internal Position(int value) { Value = value; }
            internal AccessFeature Convert(CompoundView accessPoint) => accessPoint.UniqueAccessFeature(Value);
        }
    }

}