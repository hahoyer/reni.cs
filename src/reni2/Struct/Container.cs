using System;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.IO;
using Reni.Context;
using Reni.Feature;
using Reni.Parser;
using Reni.Parser.TokenClass;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Struct
{
    /// <summary>
    /// Structured data, context free version
    /// </summary>
    [Serializable]
    internal sealed class Container : CompileSyntax, IDumpShortProvider
    {
        private static readonly string _runId = Compiler.FormattedNow + "\n";
        public static bool _isInDump;
        private static bool _isInsideFileDump;
        private static int _nextObjectId;
        private DictionaryEx<int, string> _reverseDictionaryCache;

        [Node]
        internal readonly List<ICompileSyntax> List = new List<ICompileSyntax>();

        [Node, SmartNode]
        internal readonly DictionaryEx<string, int> Dictionary = new DictionaryEx<string, int>();

        [Node, SmartNode]
        internal readonly List<int> Converters = new List<int>();

        [Node, SmartNode]
        internal readonly List<string> Properties = new List<string>();

        private readonly SimpleCache<StructFeature[]> _structFeaturesCache =
            new SimpleCache<StructFeature[]>();

        private Container(Token token)
            : base(token, _nextObjectId++) { }

        internal DictionaryEx<int, string> ReverseDictionary
        {
            get
            {
                if (_reverseDictionaryCache == null)
                    CreateReverseDictionary();
                return _reverseDictionaryCache;
            }
        }

        private StructFeature[] StructFeatures { get { return _structFeaturesCache.Find(CreateStructContainerFeatures); } }

        [DumpData(false)]
        internal ICompileSyntax this[int index] { get { return List[index]; } }

        protected internal override Result Result(ContextBase context, Category category) { return context.CreateStruct(this).ConstructorResult(category); }

        [DumpData(false)]
        protected internal override ICompileSyntax ToCompileSyntax { get { return this; } }

        [DumpData(false)]
        internal int IndexSize { get { return BitsConst.AutoSize(List.Count); } }

        protected internal override string DumpShort() { return "container." + ObjectId; }

        private StructFeature[] CreateStructContainerFeatures()
        {
            var result = new List<StructFeature>();
            for (var i = 0; i < List.Count; i++)
                result.Add(new StructFeature(i));
            return result.ToArray();
        }

        private void CreateReverseDictionary()
        {
            _reverseDictionaryCache = new DictionaryEx<int, string>();
            foreach (var pair in Dictionary)
                _reverseDictionaryCache[pair.Value] = pair.Key;
        }

        internal static IParsedSyntax Create(Token token, List<IParsedSyntax> parsed)
        {
            var result = new Container(token);
            foreach (var parsedSyntax in parsed)
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
            while (parsedSyntax is DeclarationSyntax)
            {
                var d = (DeclarationSyntax) parsedSyntax;
                Dictionary.Add(d.Name.Name, List.Count);
                parsedSyntax = d.Definition;
                if (d.IsProperty)
                    Properties.Add(d.Name.Name);
            }

            if (parsedSyntax is ConverterSyntax)
            {
                var body = ((ConverterSyntax) parsedSyntax).Body;
                parsedSyntax = (IParsedSyntax) body;
                Converters.Add(List.Count);
            }

            List.Add(parsedSyntax.ToCompileSyntax);
        }

        public string DumpPrintText(ContextBase context)
        {
            var result = "";
            for (var i = 0; i < List.Count; i++)
            {
                if (i > 0)
                    result += ";";
                if (ReverseDictionary.ContainsKey(i))
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
            if (oldResult == null || !oldResult.StartsWith(_runId))
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

        string IDumpShortProvider.DumpShort() { return DumpShort(); }

        private int Find(string name) { return Dictionary[name]; }

        private bool Defined(string name) { return Dictionary.ContainsKey(name); }

        internal SearchResult<IConverter<IConverter<IFeature, Ref>, Type>> SearchFromRefToStruct(Defineable defineable)
        {
            return SearchResult<IConverter<IConverter<IFeature, Ref>, Type>>.Create(Search(defineable));
        }

        private SearchResult<StructFeature> Search(Defineable defineable)
        {
            if (Defined(defineable.Name))
                return SearchResult<StructFeature>.Success(StructFeatures[Find(defineable.Name)],defineable);
            return defineable.SearchFromStruct().SubTrial(this);
        }

        internal SearchResult<IConverter<IContextFeature, StructContextBase>> SearchFromStructContext(Defineable defineable)
        {
            return SearchResult<IConverter<IContextFeature, StructContextBase>>.Create(Search(defineable));
        }
    }

    [Serializable]
    internal class StructFeature
        : ReniObject
            , IConverter<IConverter<IFeature, Ref>, Type>
            , IConverter<IContextFeature, StructContextBase>
    {
        private readonly int _index;

        public StructFeature(int index) { _index = index; }

        IConverter<IFeature, Ref> IConverter<IConverter<IFeature, Ref>, Type>.Convert(Type type) { return type.Context.Features[_index]; }

        IContextFeature IConverter<IContextFeature, StructContextBase>.Convert(StructContextBase context) { return context.Features[_index]; }
    }
}
