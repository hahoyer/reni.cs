using System;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.Helper.TreeViewSupport;
using Reni.Code;
using Reni.Feature;
using Reni.Parser;
using Reni.Parser.TokenClass;
using Reni.Struct;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Context
{
    /// <summary>
    /// Base class for compiler environments
    /// </summary>
    [AdditionalNodeInfo("DebuggerDumpString")]
    internal abstract class ContextBase : ReniObject, IDumpShortProvider
    {
        private static int _nextId;
        [Node]
        internal Cache _cache = new Cache();

        protected ContextBase() : base(_nextId++) {}

        [Node, DumpData(false)]
        public abstract RefAlignParam RefAlignParam { get; }
        [DumpData(false)]
        public int AlignBits { get { return RefAlignParam.AlignBits; } }
        [DumpData(false)]
        public Size RefSize { get { return RefAlignParam.RefSize; } }
        [DumpData(false)]
        public abstract Root RootContext { get; }

        [DumpData(false)]
        internal Result TopRefResult
        {
            get
            {
                if(_cache._topRefResultCache == null)
                    _cache._topRefResultCache = new Result {Code = CreateTopRefCode(), Refs = Refs.None()};

                return _cache._topRefResultCache;
            }
        }

        public string DumpShort()
        {
            return base.ToString();
        }

        public int SizeToPacketCount(Size size)
        {
            return size.SizeToPacketCount(RefAlignParam.AlignBits);
        }

        public int PacketCountDistance(ContextBase target)
        {
            var d = Distance(target);
            if(d == null)
                return -1;
            return SizeToPacketCount(d);
        }

        public Size Distance(ContextBase target)
        {
            if(target == this)
                return Size.Create(0);
            return VirtDistance(target);
        }

        public virtual Size VirtDistance(ContextBase target)
        {
            DumpMethodWithBreak("not implemented", target);
            throw new NotImplementedException();
        }

        public static Root CreateRoot()
        {
            return new Root();
        }

        internal ContextAtPosition CreateStructAtPosition(Struct.Container container, int currentCompilePosition)
        {
            return CreateStructContext(container).CreateStructAtPosition(currentCompilePosition);
        }

        private ContextAtPosition CreateStructAtPosition(int currentCompilePosition)
        {
            return _cache._structPositionCache.Find
                (
                currentCompilePosition,
                () => new ContextAtPosition((Struct.Context) this, currentCompilePosition)
                );
        }

        internal Struct.Context CreateStructContext(Struct.Container container)
        {
            return _cache._structContainerCache.Find(container,
                () => new Struct.Context(this, container));
        }

        public Function CreateFunction(TypeBase args)
        {
            return _cache._functionInstanceCache.Find(args,
                () => new Function(this, args));
        }

        internal CodeBase CreateTopRefCode()
        {
            return CodeBase.CreateTopRef(RefAlignParam);
        }

        public virtual Result CreateArgsRefResult(Category category)
        {
            NotImplementedMethod(category);
            return null;
        }

        internal Result CreateFunctionResult(Category category, SyntaxBase body)
        {
            return CreateFunctionType(body).CreateResult(category);
        }

        internal Result CreatePropertyResult(Category category, SyntaxBase body)
        {
            return CreatePropertyType(body).CreateResult(category);
        }

        internal TypeBase CreatePropertyType(SyntaxBase body)
        {
            return _cache._propertyType.Find(body,
                () => new Property(this, body));
        }

        public TypeBase CreateFunctionType(SyntaxBase body)
        {
            return _cache._functionType.Find(body,
                () => new Type.Function(this, body));
        }

        internal SearchResult<IContextFeature> SearchDefineable(DefineableToken defineableToken)
        {
            return Search(defineableToken.TokenClass).SubTrial(this);
        }

        internal virtual SearchResult<IContextFeature> Search(Defineable defineable)
        {
            return defineable.SearchContext();
        }

        internal Result VisitFirstChainElement(Category category, MemberElem memberElem)
        {
            if(memberElem.DefineableToken == null)
                return memberElem.Args.Visit(this, category);

            var contextSearchResult = SearchDefineable(memberElem.DefineableToken);
            if(contextSearchResult.IsSuccessFull)
                return contextSearchResult.Feature.VisitApply(this, category, memberElem.Args);

            if(memberElem.Args == null)
            {
                NotImplementedMethod(category, memberElem, "contextSearchResult", contextSearchResult);
                return null;
            }

            var argResult = memberElem.Args.Visit(this, category | Category.Type);
            var prefixSearchResult = argResult.Type.SearchDefineablePrefix(memberElem.DefineableToken);
            if(prefixSearchResult.IsSuccessFull)
                return prefixSearchResult.Feature.VisitApply(category, argResult);

            NotImplementedMethod(category, memberElem, "contextSearchResult", contextSearchResult, "prefixSearchResult", prefixSearchResult);
            return null;
        }

        internal Result VisitNextChainElement(Category category, MemberElem memberElem, Result formerResult)
        {
            var trace = ObjectId == -10 && memberElem.ObjectId == 2 && category.HasAll;
            StartMethodDumpWithBreak(trace, category, memberElem, formerResult);
            var refResult = formerResult.EnsureContextRef(this);
            Tracer.ConditionalBreak(trace, refResult.Dump());
            var visitedResult = ((Ref) refResult.Type).VisitNextChainElement(this, category, memberElem);
            Tracer.ConditionalBreak(trace, visitedResult.Dump());
            Tracer.Assert(visitedResult != null);
            var arglessResult = visitedResult.UseWithArg(refResult);
            return ReturnMethodDumpWithBreak(trace, arglessResult);
        }

        internal virtual bool IsChildOf(ContextBase context)
        {
            NotImplementedMethod(context);
            return true;
        }

        internal virtual CodeBase CreateRefForStruct(Struct.Type type)
        {
            NotImplementedMethod(type);
            return null;
        }

        internal List<Result> Visit(Category category, List<SyntaxBase> list)
        {
            var results = new List<Result>();
            for(var i = 0; i < list.Count; i++)
                results.Add(list[i].Visit(this, category));
            return results;
        }

        internal List<TypeBase> VisitType(List<SyntaxBase> list)
        {
            var results = new List<TypeBase>();
            for(var i = 0; i < list.Count; i++)
                results.Add(list[i].VisitType(this));
            return results;
        }

        internal class Cache
        {
            [Node]
            internal DictionaryEx<TypeBase, Function> _functionInstanceCache =
                new DictionaryEx<TypeBase, Function>();

            [Node]
            internal DictionaryEx<SyntaxBase, TypeBase> _functionType =
                new DictionaryEx<SyntaxBase, TypeBase>();

            [Node]
            internal DictionaryEx<SyntaxBase, TypeBase> _propertyType =
                new DictionaryEx<SyntaxBase, TypeBase>();

            [Node]
            internal DictionaryEx<Struct.Container, Struct.Context> _structContainerCache =
                new DictionaryEx<Struct.Container, Struct.Context>();

            [Node]
            internal DictionaryEx<int, ContextAtPosition> _structPositionCache =
                new DictionaryEx<int, ContextAtPosition>();

            [Node]
            internal Result _topRefResultCache;
        }
    }
}