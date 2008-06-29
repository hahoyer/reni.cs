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
        [Node, DumpData(false)]
        internal Cache Cache = new Cache();
        private Sequence<ContextBase> _childChainCache;

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
                if(Cache._topRefResultCache == null)
                    Cache._topRefResultCache = new Result {Code = CreateTopRefCode(), Refs = Refs.None()};

                return Cache._topRefResultCache;
            }
        }
        [DumpData(false)]
        internal Sequence<ContextBase> ChildChain
        {
            get
            {
                if(_childChainCache == null)
                    _childChainCache = ObtainChildChain();
                return _childChainCache;
            }
        }

        protected virtual Sequence<ContextBase> ObtainChildChain()
        {
            return HWString.Sequence(this);
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
                return Reni.Size.Create(0);
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
            return Cache._structPositionCache.Find
                (
                currentCompilePosition,
                () => new ContextAtPosition((Struct.Context) this, currentCompilePosition)
                );
        }

        internal Struct.Context CreateStructContext(Struct.Container container)
        {
            return Cache._structContainerCache.Find(container,
                () => new Struct.Context(this, container));
        }

        public Function CreateFunction(TypeBase args)
        {
            return Cache._functionInstanceCache.Find(args,
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

        internal Result CreateFunctionResult(Category category, ICompileSyntax body)
        {
            return CreateFunctionType(body).CreateResult(category);
        }

        internal Result CreatePropertyResult(Category category, ICompileSyntax body)
        {
            return CreatePropertyType(body).CreateResult(category);
        }

        internal TypeBase CreatePropertyType(ICompileSyntax body)
        {
            return Cache._propertyType.Find(body,() => new Property(this, body));
        }

        public TypeBase CreateFunctionType(ICompileSyntax body)
        {
            return Cache._functionType.Find(body,() => new Type.Function(this, body));
        }

        internal SearchResult<IContextFeature> SearchDefineable(DefineableToken defineableToken)
        {
            return Search(defineableToken.TokenClass).SubTrial(this);
        }

        internal virtual SearchResult<IContextFeature> Search(Defineable defineable)
        {
            return defineable.SearchContext();
        }

        private Result VisitFirstChainElement(Category category, MemberElem memberElem)
        {
            if(memberElem.DefineableToken == null)
                return Result(category,memberElem.Args);

            var contextSearchResult = SearchDefineable(memberElem.DefineableToken);
            if(contextSearchResult.IsSuccessFull)
                return contextSearchResult.Feature.VisitApply(this, category, memberElem.Args);

            if(memberElem.Args == null)
            {
                NotImplementedMethod(category, memberElem, "contextSearchResult", contextSearchResult);
                return null;
            }

            var argResult = Result(category | Category.Type,memberElem.Args);
            var prefixSearchResult = argResult.Type.SearchDefineablePrefix(memberElem.DefineableToken);
            if(prefixSearchResult.IsSuccessFull)
                return prefixSearchResult.Feature.Result(category, argResult);

            NotImplementedMethod(category, memberElem, "contextSearchResult", contextSearchResult, "prefixSearchResult",
                prefixSearchResult);
            return null;
        }

        private Result VisitNextChainElement(Category category, MemberElem memberElem, Result formerResult)
        {
            var refResult = formerResult.EnsureContextRef(this);
            var visitedResult = ((Ref) refResult.Type).VisitNextChainElement(this, category, memberElem);
            var arglessResult = visitedResult.UseWithArg(refResult);
            return arglessResult;
        }

        internal Result VisitChainElement(Category category, MemberElem memberElem, Result formerResult)
        {
            if(formerResult == null)
                return VisitFirstChainElement(category, memberElem);
            return VisitNextChainElement(category, memberElem, formerResult);
        }

        internal virtual CodeBase CreateRefForStruct(Struct.Type type)
        {
            NotImplementedMethod(type);
            return null;
        }

        internal Size Size(ICompileSyntax syntax)
        {
            return Result(Category.Size, syntax).Size;
        }

        internal List<TypeBase> Type(List<ICompileSyntax> list)
        {
            var results = new List<TypeBase>();
            for (var i = 0; i < list.Count; i++)
                results.Add(Type(list[i]));
            return results;
        }

        internal TypeBase Type(ICompileSyntax syntax)
        {
            return Result(Category.Type, syntax).Type;
        }

        internal List<Result> Result(Category category, List<ICompileSyntax> list)
        {
            var results = new List<Result>();
            for(var i = 0; i < list.Count; i++)
                results.Add(Result(category, list[i]));
            return results;
        }

        internal Result Result(Category category, ICompileSyntax syntax)
        {
            var cacheElem = Cache._resultCache.Find
                    (
                    syntax,
                    () => new CacheItem(syntax, this)
                    );
            return cacheElem.Result(category.Replendish());
        }

        internal bool IsChildOf(ContextBase parentCandidate)
        {
            return ChildChain.StartsWithAndNotEqual(parentCandidate.ChildChain);
        }

        internal bool IsStructParentOf(ContextBase child)
        {
            if(IsChildOf(child))
                return false;
            NotImplementedMethod(child);
            return false;
        }

        internal void AssertCorrectRefs(Result result)
        {
            if(result.HasRefs)
                AssertCorrectRefs(result.Refs);
            else
                Tracer.Assert(!result.HasCode);
        }

        private void AssertCorrectRefs(Refs refs)
        {
            foreach(var @ref in refs.Data)
                CheckRef(@ref);
        }

        private void CheckRef(ContextBase @ref)
        {
            Tracer.Assert(!@ref.IsChildOf(this), "context=" + Dump() + "\nref=" + @ref.Dump());
        }

        internal BitsConst Evaluate(ICompileSyntax syntax, TypeBase resultType)
        {
            var compiledResult = Result(Category.Code | Category.Type | Category.Refs, syntax);
            var convertedResult = compiledResult.ConvertTo(resultType);
            return convertedResult.Evaluate();
        }

        internal CodeBase Code(ICompileSyntax syntax)
        {
            return Result(Category.Code, syntax).Code;
        }

        internal Result ApplyResult(Category category, ICompileSyntax @object, Result.GetResultFromType apply)
        {
            var objectType = Type(@object);
            if (objectType.IsRef(RefAlignParam))
                return apply(objectType).UseWithArg(Result(category, @object));

            Tracer.Assert(category.HasInternal);
            var objectRefType = objectType.CreateRef(RefAlignParam);
            var objectRefResult = objectRefType
                .CreateResult(
                category, 
                () => CodeBase.CreateTopRef(RefAlignParam), 
                () => Result(Category.ForInternal, @object)
                );
            return apply(objectRefType).UseWithArg(objectRefResult);
        }
    }

    internal class Cache
    {
        [Node]
        internal readonly DictionaryEx<TypeBase, Function> _functionInstanceCache = new DictionaryEx<TypeBase, Function>();
        [Node]
        internal readonly DictionaryEx<ICompileSyntax, TypeBase> _functionType = new DictionaryEx<ICompileSyntax, TypeBase>();
        [Node]
        internal readonly DictionaryEx<ICompileSyntax, TypeBase> _propertyType = new DictionaryEx<ICompileSyntax, TypeBase>();
        [Node]
        internal readonly DictionaryEx<Struct.Container, Struct.Context> _structContainerCache = new DictionaryEx<Struct.Container, Struct.Context>();
        [Node]
        internal readonly DictionaryEx<int, ContextAtPosition> _structPositionCache = new DictionaryEx<int, ContextAtPosition>();
        [Node]
        internal Result _topRefResultCache;
        [Node]
        internal readonly DictionaryEx<ICompileSyntax, CacheItem> _resultCache = new DictionaryEx<ICompileSyntax, CacheItem>();

    }
}