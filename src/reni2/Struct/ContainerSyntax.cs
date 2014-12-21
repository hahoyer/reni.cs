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
using Reni.TokenClasses;

namespace Reni.Struct
{
    /// <summary>
    ///     Structured data, context free version
    /// </summary>
    sealed class ContainerSyntax : CompileSyntax
    {
        readonly Data[] _data;
        static readonly string _runId = Compiler.FormattedNow + "\n";
        public static bool IsInContainerDump;
        static bool _isInsideFileDump;
        static int _nextObjectId;

        [Node]
        internal CompileSyntax[] Statements { get { return _data.Select(s => s.Statement).ToArray(); } }

        [DisableDump]
        internal int EndPosition { get { return Statements.Length; } }

        internal ContainerSyntax
            (
            SourcePart token,
            Syntax[] statements)
            : base(token, _nextObjectId++) { _data = statements.Select((s, i) => new Data(s, i)).ToArray(); }

        [DisableDump]
        internal override CompileSyntax ToCompiledSyntax { get { return this; } }
        [DisableDump]
        internal override bool? Hllw
        {
            get
            {
                return Statements
                    .All(syntax => syntax.Hllw == true);
            }
        }

        [DisableDump]
        internal Size IndexSize { get { return Size.AutoSize(Statements.Length); } }

        protected override string GetNodeDump() { return "container." + ObjectId; }

        [DisableDump]
        protected override ParsedSyntax[] Children { get { return Statements.ToArray<ParsedSyntax>(); } }
        [DisableDump]
        public string[] Names { get { return _data.SelectMany(s => s.Names).ToArray(); } }
        [DisableDump]
        public int[] Converters { get { return _data.SelectMany((s, i) => s.IsConverter ? new[] {i} : new int[0]).ToArray(); } }

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

        internal StructurePosition Find(string name)
        {
            if(name == null)
                return null;
            var result = _data.SingleOrDefault(s => s.Defines(name));
            if(result == null)
                return null;

            return new StructurePosition(result.Position);
        }

        internal override Result ObtainResult(ContextBase context, Category category)
        {
            return context
                .UniqueContainerContext(this)
                .Result(category);
        }

        sealed class Data : DumpableObject
        {
            readonly Syntax _rawStatement;
            public readonly int Position;
            readonly ValueCache<string[]> _namesCache;
            readonly ValueCache<CompileSyntax> _statement;

            public Data(Syntax rawStatement, int position)
            {
                _rawStatement = rawStatement;
                Position = position;
                _statement = new ValueCache<CompileSyntax>(GetStatement);
                _namesCache = new ValueCache<string[]>(GetNames);
            }

            public CompileSyntax Statement { get { return _statement.Value; } }
            public bool Defines(string name) { return Names.Contains(name); }
            public bool IsConverter { get { return _rawStatement is ConverterSyntax; } }
            public IEnumerable<string> Names { get { return _namesCache.Value; } }
            public bool IsConst { get { return !(_rawStatement.IsEnableReassignSyntax); } }

            CompileSyntax GetStatement() { return _rawStatement.ContainerStatementToCompileSyntax; }
            string[] GetNames() { return _rawStatement.GetDeclarations().ToArray(); }
        }

        public bool IsConst(int position) { return _data[position].IsConst; }
    }


    sealed class StructurePosition : DumpableObject
    {
        [EnableDump]
        internal readonly int Position;

        internal StructurePosition(int position) { Position = position; }

        internal AccessFeature Convert(ContainerView accessPoint) { return accessPoint.UniqueAccessFeature(Position); }
    }
}