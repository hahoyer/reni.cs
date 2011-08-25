using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.IO;
using HWClassLibrary.TreeStructure;
using Reni.Basics;
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
        private readonly CompileSyntax[] _statements;
        private readonly DictionaryEx<string, int> _dictionary;
        private readonly int[] _converters;
        private readonly int[] _properties;
        private static readonly string _runId = Compiler.FormattedNow + "\n";
        public static bool IsInContainerDump;
        private static bool _isInsideFileDump;
        private static int _nextObjectId;

        [Node]
        internal CompileSyntax[] Statements { get { return _statements; } }

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

        private Container(TokenData leftToken, TokenData rightToken, CompileSyntax[] statements, DictionaryEx<string, int> dictionary, int[] converters, int[] properties)
            : base(leftToken,_nextObjectId++)
        {
            _firstToken = leftToken;
            _lastToken = rightToken;
            _statements = statements;
            _dictionary = dictionary;
            _converters = converters;
            _properties = properties;
        }

        internal override CompileSyntax ToCompiledSyntax() { return this; }

        [DisableDump]
        internal int IndexSize { get { return BitsConst.AutoSize(Statements.Length); } }

        internal override string DumpShort() { return "container." + ObjectId; }

        sealed class PreContainer: ReniObject
        {
            private readonly List<CompileSyntax> _list = new List<CompileSyntax>();
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
                    parsedSyntax = body;
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

        internal override Result ObtainResult(ContextBase context, Category category)
        {
            var innerResult = ConstructionResult(category - Category.Type, context, 0, EndPosition);
            return context.UniqueContainerContext(this).Result(category, innerResult);
        }

        private Result InternalInnerResult(Category category, ContextBase parent, int accessPosition, int position)
        {
            var trace = ObjectId==-1 && accessPosition==1 && position == 0 && (category.HasArgs || category.HasCode);
            StartMethodDump(trace,category,parent,accessPosition,position);
            try
            {
                var uniqueChildContext = parent
                    .UniqueChildContext(this, accessPosition);
                Dump("Statements[position]", Statements[position]);
                BreakExecution();
                var result = Statements[position]
                    .Result(uniqueChildContext, category.Typed);
                return ReturnMethodDump(result
                    .AutomaticDereference(),true);
            }
            finally
            {
                EndMethodDump();
            }
        }

        private Result InnerResult(Category category, ContextBase parent, int accessPosition, int position)
        {
            Tracer.Assert(!(category.HasCode));
            return InternalInnerResult(category, parent, accessPosition, position);
        }

        internal Result ConstructionResult(Category category, ContextBase parent, int fromPosition, int fromNotPosition)
        {
            var result = TypeBase.VoidResult(category);
            for(var position = fromPosition; position < fromNotPosition; position++)
                result = result.Sequence(ConstructorResult(category, parent, position));
            return result;
        }

        internal bool IsDataLess(ContextBase context, int fromPosition, int fromNotPosition)
        {
            var listQuick = Statements
                .Select(s => new {Statement=s, IsDataLess=s.QuickIsDataLess(context)})
                .Where(ss=> ss.IsDataLess != true)
                .ToArray();
            if (listQuick.Any(ss => ss.IsDataLess == false))
                return false;

            var listFlat = listQuick
                .Select(s => new {Statement = s, IsDataLess = s.Statement.FlatIsDataLess(context)})
                .Where(s=>s.IsDataLess != true);
            if (listFlat.Any(ss => ss.IsDataLess == false))
                return false;


            NotImplementedMethod(context,fromPosition,fromNotPosition);
            return false;
        }

        private Result ConstructorResult(Category category, ContextBase parent, int position)
        {
            StartMethodDump(ObjectId == 0 && position == 0 && category.HasIsDataLess, category, parent, position);
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
        
        internal new bool IsLambda(int position) { return Statements[position].IsLambda; }
        internal bool IsProperty(int position) { return Properties.Contains(position); }

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