using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.IO;
using HWClassLibrary.TreeStructure;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Parser;
using Reni.ReniParser;
using Reni.Syntax;
using Reni.TokenClasses;
using Reni.Type;

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
        private readonly ICompileSyntax[] _statements;
        private readonly DictionaryEx<string, int> _dictionary;
        private readonly int[] _converters;
        private readonly int[] _properties;
        private static readonly string _runId = Compiler.FormattedNow + "\n";
        public static bool IsInContainerDump;
        private static bool _isInsideFileDump;
        private static int _nextObjectId;
        private DictionaryEx<int, string> _reverseDictionaryCache;
        private readonly DictionaryEx<int, Context> _contextCache;
        private readonly DictionaryEx<ContextBase, ContainerContextObject> _containerContextCache;

        [Node]
        internal ICompileSyntax[] Statements { get { return _statements; } }

        [DisableDump]
        internal int EndPosition { get { return Statements.Length; } }

        [Node, SmartNode]
        internal DictionaryEx<string, int> Dictionary { get { return _dictionary; } }

        [Node, SmartNode]
        internal int[] Converters { get { return _converters; } }

        [Node, SmartNode]
        internal int[] Properties { get { return _properties; } }
        
        protected override TokenData GetFirstToken() { return _firstToken; }

        protected override TokenData GetLastToken() { return _lastToken; }

        private Container(TokenData leftToken, TokenData rightToken, ICompileSyntax[] statements, DictionaryEx<string, int> dictionary, int[] converters, int[] properties)
            : base(leftToken,_nextObjectId++)
        {
            _firstToken = leftToken;
            _lastToken = rightToken;
            _statements = statements;
            _dictionary = dictionary;
            _converters = converters;
            _properties = properties;
            _contextCache = new DictionaryEx<int, Context>(position => new Context(this, position));
            _containerContextCache = new DictionaryEx<ContextBase, ContainerContextObject>(parent => new ContainerContextObject(this, parent));
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

        internal override ICompileSyntax ToCompiledSyntax() { return this; }

        [DisableDump]
        internal int IndexSize { get { return BitsConst.AutoSize(Statements.Length); } }

        internal override string DumpShort() { return "container." + ObjectId; }

        private void CreateReverseDictionary()
        {
            _reverseDictionaryCache = new DictionaryEx<int, string>();
            foreach(var pair in Dictionary)
                _reverseDictionaryCache[pair.Value] = pair.Key;
        }

        sealed class PreContainer: ReniObject
        {
            private readonly List<ICompileSyntax> _list = new List<ICompileSyntax>();
            private readonly DictionaryEx<string, int> _dictionary = new DictionaryEx<string, int>();
            private readonly List<int> _converters = new List<int>();
            private readonly List<int> _properties = new List<int>();

            public void Add(IParsedSyntax parsedSyntax)
            {
                while (parsedSyntax is DeclarationSyntax)
                {
                    var d = (DeclarationSyntax)parsedSyntax;
                    _dictionary.Add(d.Defineable.Name, _list.Count);
                    parsedSyntax = d.Definition;
                    if (d.IsProperty)
                        _properties.Add(_list.Count);
                }

                if (parsedSyntax is ConverterSyntax)
                {
                    var body = ((ConverterSyntax)parsedSyntax).Body;
                    parsedSyntax = (ReniParser.ParsedSyntax)body;
                    _converters.Add(_list.Count);
                }

                _list.Add(((ReniParser.ParsedSyntax)parsedSyntax).ToCompiledSyntax());
            }

            public Container ToContainer(TokenData leftToken, TokenData rightToken) 
            { 
                return new Container(leftToken,rightToken,_list.ToArray(),_dictionary,_converters.ToArray(),_properties.ToArray());
            }
        }


        internal static Container Create(TokenData leftToken, TokenData rightToken, List<IParsedSyntax> parsed)
        {
            var result = new PreContainer();
            foreach(var parsedSyntax in parsed)
                result.Add(parsedSyntax);
            return result.ToContainer(leftToken, rightToken);
        }

        internal static Container Create(TokenData leftToken, TokenData rightToken, ReniParser.ParsedSyntax parsedSyntax)
        {
            var result = new PreContainer();
            result.Add(parsedSyntax);
            return result.ToContainer(leftToken, rightToken);
        }

        internal static Container Create(ReniParser.ParsedSyntax parsedSyntax) { return Create(parsedSyntax.FirstToken, parsedSyntax.LastToken, parsedSyntax); }

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
            {
                var position = Dictionary[name];
                return new StructureFeature(position);
            }
            return null;
        }

        internal ISearchPath<IFeature, StructureType> SearchFromRefToStruct(Defineable defineable)
        {
            return FindStructFeature(defineable.Name);
        }

        internal ISearchPath<IContextFeature, ContainerContextObject> SearchFromStructContext(Defineable defineable)
        {
            return FindStructFeature(defineable.Name);
        }

        internal override Result Result(ContextBase context, Category category)
        {
            var innerResult = ConstructionResult(category - Category.Type, context, 0, EndPosition);
            return context.UniqueContainerContext(this).Result(category, innerResult);
        }

        internal Result InternalInnerResult(Category category, ContextBase parent, int accessPosition, int position)
        {
            var trace = ObjectId==-10 && accessPosition==3 && position == 2 && category.HasRefs;
            StartMethodDump(trace,category,parent,accessPosition,position);
            try
            {

                var uniqueChildContext = parent
                    .UniqueChildContext(this, accessPosition);
                var result = uniqueChildContext
                    .Result(category | Category.Type, Statements[position]);
                return ReturnMethodDump(result
                    .AutomaticDereference(),true);
            }
            finally
            {
                EndMethodDump();
            }
        }

        internal Result InnerResult(Category category, ContextBase parent, int accessPosition, int position)
        {
            Tracer.Assert(!(category.HasCode));
            return InternalInnerResult(category, parent, accessPosition, position);
        }

        internal Context UniqueContext(int position) { return _contextCache.Find(position); }

        internal ContainerContextObject UniqueContainerContext(ContextBase parent)
        {
            return _containerContextCache.Find(parent);
        }
        
        internal Result ConstructionResult(Category category, ContextBase parent, int fromPosition, int fromNotPosition)
        {
            var result = TypeBase.VoidResult(category);
            for(var position = fromPosition; position < fromNotPosition; position++)
                result = result.Sequence(ConstructorResult(category, parent, position));
            return result;
        }

        private Result ConstructorResult(Category category, ContextBase parent, int position)
        {
            StartMethodDump(ObjectId == -1 && position == 1 && category.HasCode, category, parent, position);
            try
            {
                var internalInnerResult = InternalInnerResult(category, parent, position + 1, position);
                Dump("internalInnerResult", internalInnerResult);
                var alignedResult = internalInnerResult.Align(parent.RefAlignParam.AlignBits);
                Dump("alignedResult", alignedResult);
                BreakExecution();
                var result = alignedResult.LocalBlock(category);
                return ReturnMethodDump(result,true);
            }
            finally
            {
                EndMethodDump();
            }
        }

        internal Size InnerSize(ContextBase parent, int position) { return InnerResult(Category.Size, parent, EndPosition, position).Size; }
        internal Size ConstructionSize(ContextBase parent, int fromPosition, int fromNotPosition) { return ConstructionResult(Category.Size, parent, fromPosition, fromNotPosition).Size; }
        internal TypeBase InnerType(ContextBase parent, int accessPosition, int position) { return InnerResult(Category.Type, parent, accessPosition, position).Type; }
        
        internal bool IsLambda(int position) { return Statements[position].IsLambda; }
        internal bool IsProperty(int position) { return Properties.Contains(position); }

        internal bool IsZeroSized(ContextBase parent, int fromPosition, int fromNotPosition)
        {
            for(var i = fromPosition; i < fromNotPosition; i++)
                if (!IsLambda(i) && !InnerSize(parent, i).IsZero)
                    return false;
            return true;
        }
    }

    internal interface IStructFeature
        : ISearchPath<IContextFeature, ContainerContextObject>, ISearchPath<IFeature, StructureType>
    {}

    [Serializable]
    internal sealed class StructureFeature : ReniObject, IStructFeature
    {
        private readonly int _position;

        public StructureFeature(int position)
        {
            _position = position;
        }

        IContextFeature ISearchPath<IContextFeature, ContainerContextObject>.Convert(ContainerContextObject contextObject)
        {
            return Convert(contextObject.ToStructure);
        }

        private AccessFeature Convert(Structure contextObject) { return new AccessFeature(contextObject, _position); }

        IFeature ISearchPath<IFeature, StructureType>.Convert(StructureType structureType)
        {
            return Convert(structureType.Structure);
        }
    }
}