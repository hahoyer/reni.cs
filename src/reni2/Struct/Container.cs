using System.Collections.Generic;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.Helper.TreeViewSupport;
using HWClassLibrary.IO;
using Reni.Context;
using Reni.Parser;
using Reni.Parser.TokenClass;
using Reni.Syntax;

namespace Reni.Struct
{
    /// <summary>
    /// Structured data, context free version
    /// </summary>
    internal sealed class Container : ParsedSyntax, IDumpShortProvider, ICompileSyntax
    {
        private static readonly string _runId = Compiler.FormattedNow + "\n";
        public static bool _isInDump;
        private static bool _isInsideFileDump;
        private static int _nextObjectId;
        private DictionaryEx<int, string> _reverseDictionaryCache;

        internal readonly List<ICompileSyntax> ConverterList = new List<ICompileSyntax>();
        internal readonly DictionaryEx<string, int> Dictionary = new DictionaryEx<string, int>();
        internal readonly List<ICompileSyntax> List = new List<ICompileSyntax>();

        private Container(Token token) : base(token, _nextObjectId++) {}

        [Node, DumpData(false)]
        internal DictionaryEx<int, string> ReverseDictionary
        {
            get
            {
                if(_reverseDictionaryCache == null)
                    CreateReverseDictionary();
                return _reverseDictionaryCache;
            }
        }

        [DumpData(false)]
        internal ICompileSyntax this[int index] { get { return List[index]; } }

        public Result Result(ContextBase context, Category category)
        {
            return context.CreateStruct(this).CreateType().ConstructorResult(category);
        }

        string ICompileSyntax.DumpShort()
        {
            return DumpShort();
        }

        [DumpData(false)]
        internal protected override ICompileSyntax ToCompileSyntax { get { return this; } }
        [DumpData(false)]
        internal int IndexSize { get { return BitsConst.AutoSize(List.Count); } }

        string ICompileSyntax.FilePosition()
        {
            return FilePosition();
        }

        internal protected override string DumpShort()
        {
            return "container." + ObjectId;
        }

        private void CreateReverseDictionary()
        {
            _reverseDictionaryCache = new DictionaryEx<int, string>();
            foreach(var pair in Dictionary)
                _reverseDictionaryCache[pair.Value] = pair.Key;
        }

        internal static IParsedSyntax Create(Token token, List<IParsedSyntax> parsed)
        {
            var result = new Container(token);
            foreach(var parsedSyntax in parsed)
                result.Add(parsedSyntax);
            return result;
        }

        internal static IParsedSyntax Create(Token token, IParsedSyntax parsedSyntax)
        {
            var result = new Container(token);
            result.Add(parsedSyntax);
            return result;
        }

        private void Add(IParsedSyntax parsedSyntax)
        {
            while(parsedSyntax is DeclarationSyntax)
            {
                var d = (DeclarationSyntax) parsedSyntax;
                Dictionary.Add(d.Name.Name, List.Count);
                parsedSyntax = d.Definition;
            }
            if(parsedSyntax is ConverterSyntax)
                ConverterList.Add(((ConverterSyntax) parsedSyntax).Body);
            else
                List.Add(parsedSyntax.ToCompileSyntax);
        }

        public string DumpPrintText(ContextBase context)
        {
            var result = "";
            for(var i = 0; i < List.Count; i++)
            {
                if(i > 0)
                    result += ";";
                if(ReverseDictionary.ContainsKey(i))
                    result += ReverseDictionary[i] + ": ";
                result += context.Type(List[i]);
            }
            return result;
        }

        public override string DumpData()
        {
            var isInsideFileDump = _isInsideFileDump;
            _isInsideFileDump = true;
            var result = isInsideFileDump ? DumpDataToString() : DumpDataToFile();
            _isInsideFileDump = isInsideFileDump;
            return result;
        }

        private string DumpDataToFile()
        {
            var dumpFile = File.m("struct." + ObjectId);
            var oldResult = dumpFile.String;
            var newResult = (_runId + DumpDataToString()).Replace("\n", "\r\n");
            if(oldResult == null || !oldResult.StartsWith(_runId))
            {
                oldResult = newResult;
                dumpFile.String = oldResult;
            }
            else
                Tracer.Assert(oldResult == newResult);
            return Tracer.FilePosn(dumpFile.FullName, 1, 0, "see there") + "\n";
        }

        private string DumpDataToString()
        {
            var isInDump = _isInDump;
            _isInDump = true;
            var result = base.DumpData();
            _isInDump = isInDump;
            return result;
        }

        string IDumpShortProvider.DumpShort()
        {
            return DumpShort();
        }
    }
}