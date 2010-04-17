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
        public static bool IsInContainerDump;
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

        private Container(Token token)
            : base(token, _nextObjectId++)
        {
            EmptyList = new EmptyList(token);
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

        [DumpData(false)]
        internal ICompileSyntax this[int index] { get { return List[index]; } }

        protected internal override Result Result(ContextBase context, Category category)
        {
            return context.CreateStruct(this).ConstructorResult(category);
        }

        protected override ICompileSyntax ToCompiledSyntax() { return this; }

        [DumpData(false)]
        internal int IndexSize { get { return BitsConst.AutoSize(List.Count); } }

        [DumpData(false)]
        internal readonly EmptyList EmptyList;

        protected internal override string DumpShort()
        {
            return "container." + ObjectId;
        }

        private void CreateReverseDictionary()
        {
            _reverseDictionaryCache = new DictionaryEx<int, string>();
            foreach(var pair in Dictionary)
                _reverseDictionaryCache[pair.Value] = pair.Key;
        }

        internal static Container Create(Token token, List<IParsedSyntax> parsed)
        {
            var result = new Container(token);
            foreach(var parsedSyntax in parsed)
                result.Add(parsedSyntax);
            return result;
        }

        internal static Container Create(Token token, IParsedSyntax parsedSyntax)
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
                if(d.IsProperty)
                    Properties.Add(d.Name.Name);
            }

            if(parsedSyntax is ConverterSyntax)
            {
                var body = ((ConverterSyntax) parsedSyntax).Body;
                parsedSyntax = (IParsedSyntax) body;
                Converters.Add(List.Count);
            }

            List.Add(parsedSyntax.ToCompiledSyntax());
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

        string IDumpShortProvider.DumpShort()
        {
            return DumpShort();
        }

        private IStructFeature FindStructFeature(string name)
        {
            if (Dictionary.ContainsKey(name))
                return new StructFeature(Dictionary[name], Properties.Contains(name));
            return null;
        }

        internal ISearchPath<ISearchPath<IFeature, Reference>, FullContextType> SearchFromRefToStruct(Defineable defineable)
        {
            return FindStructFeature(defineable.Name);
        }

        internal ISearchPath<IContextFeature, Context> SearchFromStructContext(Defineable defineable)
        {
            return FindStructFeature(defineable.Name);
        }

    }

    internal interface IStructFeature
        : ISearchPath<ISearchPath<IFeature, Reference>, FullContextType>
          , ISearchPath<IContextFeature, Context>
    {
    }

    [Serializable]
    internal class StructFeature : ReniObject, IStructFeature
    {
        private readonly int _index;
        private readonly bool _isProperty;

        public StructFeature(int index, bool isProperty)
        {
            _index = index;
            _isProperty = isProperty;
        }

        ISearchPath<IFeature, Reference> ISearchPath<ISearchPath<IFeature, Reference>, FullContextType>.Convert(FullContextType type)
        {
            return type.Context.Features[_index].ToProperty(_isProperty);
        }

        IContextFeature ISearchPath<IContextFeature, Context>.Convert(Context context)
        {
            return context.Features[_index].ToProperty(_isProperty);
        }
    }
}