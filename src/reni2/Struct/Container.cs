using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.IO;
using HWClassLibrary.TreeStructure;
using Reni.Context;
using Reni.Feature;
using Reni.Parser;
using Reni.ReniParser;
using Reni.ReniParser.TokenClasses;
using Reni.Syntax;

namespace Reni.Struct
{
    /// <summary>
    ///     Structured data, context free version
    /// </summary>
    [Serializable]
    internal sealed class Container : CompileSyntax, IDumpShortProvider
    {
        private readonly TokenData _firstToken;
        private readonly TokenData _lastToken;
        private static readonly string _runId = Compiler.FormattedNow + "\n";
        public static bool IsInContainerDump;
        private static bool _isInsideFileDump;
        private static int _nextObjectId;
        private DictionaryEx<int, string> _reverseDictionaryCache;

        [Node]
        internal readonly List<ICompileSyntax> List = new List<ICompileSyntax>();

        [Node, SmartNode]
        internal readonly DictionaryEx<string, int> Dictionary;

        [Node, SmartNode]
        internal readonly List<int> Converters = new List<int>();

        [Node, SmartNode]
        internal readonly List<string> Properties = new List<string>();

        protected override TokenData GetFirstToken() { return _firstToken; }

        protected override TokenData GetLastToken() { return _lastToken; }

        private Container(TokenData leftToken, TokenData rightToken)
            : base(leftToken, _nextObjectId++)
        {
            _firstToken = leftToken;
            _lastToken = rightToken;
            Dictionary = new DictionaryEx<string, int>();
        }

        internal DictionaryEx<int, string> ReverseDictionary
        {
            get
            {
                if(_reverseDictionaryCache == null)
                    CreateReverseDictionary();
                return _reverseDictionaryCache;
            }
        }

        protected internal override Result Result(ContextBase context, Category category) { return context.CreateStruct(this).ConstructorResult(category); }

        internal override ICompileSyntax ToCompiledSyntax() { return this; }

        [IsDumpEnabled(false)]
        internal int IndexSize { get { return BitsConst.AutoSize(List.Count); } }

        internal override string DumpShort() { return "container." + ObjectId; }

        private void CreateReverseDictionary()
        {
            _reverseDictionaryCache = new DictionaryEx<int, string>();
            foreach(var pair in Dictionary)
                _reverseDictionaryCache[pair.Value] = pair.Key;
        }

        internal static Container Create(TokenData leftToken, TokenData rightToken, List<IParsedSyntax> parsed)
        {
            var result = new Container(leftToken, rightToken);
            foreach(var parsedSyntax in parsed)
                result.Add(parsedSyntax);
            return result;
        }

        internal static Container Create(TokenData leftToken, TokenData rightToken, ReniParser.ParsedSyntax parsedSyntax)
        {
            var result = new Container(leftToken, rightToken);
            result.Add(parsedSyntax);
            return result;
        }

        internal static Container Create(ReniParser.ParsedSyntax parsedSyntax) { return Create(parsedSyntax.FirstToken, parsedSyntax.LastToken, parsedSyntax); }

        private void Add(IParsedSyntax parsedSyntax)
        {
            while(parsedSyntax is DeclarationSyntax)
            {
                var d = (DeclarationSyntax) parsedSyntax;
                Dictionary.Add(d.Defineable.Name, List.Count);
                parsedSyntax = d.Definition;
                if(d.IsProperty)
                    Properties.Add(d.Defineable.Name);
            }

            if(parsedSyntax is ConverterSyntax)
            {
                var body = ((ConverterSyntax) parsedSyntax).Body;
                parsedSyntax = (ReniParser.ParsedSyntax) body;
                Converters.Add(List.Count);
            }

            List.Add(((ReniParser.ParsedSyntax) parsedSyntax).ToCompiledSyntax());
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
            var isInDump = IsInContainerDump;
            IsInContainerDump = true;
            var result = base.DumpData();
            IsInContainerDump = isInDump;
            return result;
        }

        string IDumpShortProvider.DumpShort() { return DumpShort(); }

        private IStructFeature FindStructFeature(string name)
        {
            if(Dictionary.ContainsKey(name))
                return new StructFeature(Dictionary[name], Properties.Contains(name));
            return null;
        }

        internal ISearchPath<IFeature, Type> SearchFromRefToStruct(Defineable defineable) { return FindStructFeature(defineable.Name); }

        internal ISearchPath<IContextFeature, Context> SearchFromStructContext(Defineable defineable) { return FindStructFeature(defineable.Name); }
    }

    internal interface IStructFeature
        : ISearchPath<IContextFeature, Context>, ISearchPath<IFeature, Type>
    {}

    [Serializable]
    internal sealed class StructFeature : ReniObject, IStructFeature
    {
        private readonly int _index;
        private readonly bool _isProperty;

        public StructFeature(int index, bool isProperty)
        {
            _index = index;
            _isProperty = isProperty;
        }

        IContextFeature ISearchPath<IContextFeature, Context>.Convert(Context context)
        {
            context.AssertValid();
            return context
                .Features[_index]
                .ToProperty(_isProperty);
        }

        IFeature ISearchPath<IFeature, Type>.Convert(Type type)
        {
            return type
                .Context
                .Features[_index]
                .ToProperty(_isProperty);
        }
    }
}