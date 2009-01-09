using System;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Code;
using Reni.Feature;
using Reni.Parser;
using Reni.Parser.TokenClass;
using Reni.Struct;
using Reni.Syntax;
using Reni.Type;
using Container=Reni.Struct.Container;

namespace Reni.Context
{
    /// <summary>
    /// Base class for compiler environments
    /// </summary>
    [Serializable]
    internal abstract class ContextBase : ReniObject, IDumpShortProvider, IIconKeyProvider
    {
        private static int _nextId;

        [Node, DumpData(false)]
        internal Cache Cache = new Cache();

        private Sequence<ContextBase> _childChainCache;

        protected ContextBase()
            : base(_nextId++) { }

        [Node, DumpData(false)]
        internal abstract RefAlignParam RefAlignParam { get; }

        [DumpData(false)]
        public int AlignBits { get { return RefAlignParam.AlignBits; } }

        [DumpData(false)]
        public Size RefSize { get { return RefAlignParam.RefSize; } }

        [DumpData(false)]
        internal abstract Root RootContext { get; }

        [DumpData(false)]
        internal Result TopRefResult
        {
            get
            {
                return
                    Cache._topRefResultCache.Find(
                        () => new Result {Code = CreateTopRefCode(), Refs = Refs.None()});
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

        internal protected virtual Sequence<ContextBase> ObtainChildChain() { return HWString.Sequence(this); }

        internal virtual string DumpShort() { return base.ToString(); }

        internal int SizeToPacketCount(Size size) { return size.SizeToPacketCount(RefAlignParam.AlignBits); }

        internal static Root CreateRoot() { return new Root(); }

        internal Function CreateFunction(TypeBase args)
        {
            return Cache._functionInstanceCache.Find(args,
                                                     () => new Function(this, args));
        }

        internal PendingResultContext CreatePendingResultContext()
        {
            return Cache._pendingResultContext.Find(()
                                                    => new PendingResultContext(this));
        }

        internal FullContext CreateStruct(Container container)
        {
            return Cache._structContainerCache.Find(container,
                                                    () => new FullContext(this, container));
        }

        internal CodeBase CreateTopRefCode() { return CodeBase.CreateTopRef(RefAlignParam); }

        internal virtual Result CreateArgsRefResult(Category category)
        {
            NotImplementedMethod(category);
            return null;
        }

        internal Result CreateFunctionResult(Category category, ICompileSyntax body) { return CreateFunctionType(body).CreateResult(category); }
        internal Result CreatePropertyResult(Category category, ICompileSyntax body) { return CreatePropertyType(body).CreateResult(category); }
        internal TypeBase CreatePropertyType(ICompileSyntax body) { return Cache._propertyType.Find(body, () => new Property(this, body)); }
        internal TypeBase CreateFunctionType(ICompileSyntax body) { return Cache._functionType.Find(body, () => new Type.Function(this, body)); }
        internal SearchResult<IContextFeature> SearchDefineable(DefineableToken defineableToken) { return Search(defineableToken.TokenClass).SubTrial(this); }
        internal virtual SearchResult<IContextFeature> Search(Defineable defineable) { return defineable.SearchContext(); }
        internal Size Size(ICompileSyntax syntax) { return Result(Category.Size, syntax).Size; }

        internal TypeBase Type(ICompileSyntax syntax)
        {
            var result = Result(Category.Type, syntax).Type;
            Tracer.Assert(result != null);
            return result;
        }

        internal List<Result> Result(Category category, List<ICompileSyntax> list)
        {
            var results = new List<Result>();
            for(var i = 0; i < list.Count; i++)
                results.Add(Result(category, list[i]));
            return results;
        }

        //[DebuggerHidden]
        internal Result Result(Category category, ICompileSyntax syntax)
        {
            return Cache._resultCache.Find
                (
                syntax,
                () => CreateCacheElement(syntax)
                )
                .Result(category);
        }

        private CacheItem CreateCacheElement(ICompileSyntax syntax)
        {
            var result = new CacheItem(syntax, this);
            syntax.AddToCacheForDebug(this, result);
            return result;
        }

        internal bool IsChildOf(ContextBase parentCandidate) { return ChildChain.StartsWithAndNotEqual(parentCandidate.ChildChain); }

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
                AssertCorrectRefs(result.Refs.Data);
            else if(result.HasCode)
                AssertCorrectRefs(result.Code.Refs);
        }

        private void AssertCorrectRefs(IEnumerable<IRefInCode> refs)
        {
            foreach(var @ref in refs)
                CheckRef(@ref);
        }

        private void CheckRef(IRefInCode @ref)
        {
            Tracer.Assert(!@ref
                               .IsChildOf(this), "context=" + Dump() + "\nref="
                                                 + @ref.Dump());
        }

        internal BitsConst Evaluate(ICompileSyntax syntax, TypeBase resultType)
        {
            var compiledResult = Result(Category.Code | Category.Type | Category.Refs, syntax);
            var convertedResult = compiledResult.ConvertTo(resultType);
            return convertedResult.Evaluate();
        }

        internal CodeBase Code(ICompileSyntax syntax) { return Result(Category.Code, syntax).Code; }

        internal Result ApplyResult(Category category, ICompileSyntax @object, Func<TypeBase, Result> apply)
        {
            var objectResult = ResultAsRef(category | Category.Type, @object);
            return apply(objectResult.Type)
                .Align(AlignBits)
                .UseWithArg(objectResult);
        }

        internal Result ConvertToSequence(Category category, ICompileSyntax syntax, TypeBase elementType) { return ConvertToViaRef(category, syntax, Type(syntax).CreateSequenceType(elementType)); }

        private Result ConvertToViaRef(Category category, ICompileSyntax syntax, TypeBase target) { return ResultAsRef(category | Category.Type, syntax).ConvertTo(target).Align(AlignBits); }

        internal Result ResultAsRef(Category category, ICompileSyntax syntax)
        {
            var result = Result(category | Category.Type, syntax);
            if(result.Type.IsRef(RefAlignParam))
                return Result(category, syntax);

            return result.CreateAutomaticRefResult(category, result.Type.CreateAutomaticRef(RefAlignParam));
        }

        internal Result ConvertedRefResult(Category category, ICompileSyntax syntax, AutomaticRef target)
        {
            var result = Result(category | Category.Type, syntax);
            if(result.Type.IsRefLike(target))
                return target.CreateResult(category, Result(category & (Category.Code | Category.Refs), syntax));

            if(result.Type.IsRef(RefAlignParam))
            {
                var convertedResult = result.ConvertTo(target.Target);
                NotImplementedMethod(category, syntax, target, "type", result.Type, "result", result, "convertedResult",
                                     convertedResult);
                return result;
            }
            return result.ConvertTo(target.AlignedTarget).CreateAutomaticRefResult(category, target);
        }

        string IDumpShortProvider.DumpShort() { return DumpShort(); }

        /// <summary>
        /// Gets the icon key.
        /// </summary>
        /// <value>The icon key.</value>
        string IIconKeyProvider.IconKey { get { return "Context"; } }

        internal virtual IStructContext FindStruct()
        {
            NotImplementedMethod();
            return null;
        }

        internal Result PrefixResult(Category category, DefineableToken defineableToken, ICompileSyntax right)
        {
            var contextSearchResult = SearchDefineable(defineableToken);
            if(contextSearchResult.IsSuccessFull)
                return contextSearchResult.Feature.ApplyResult(this, category, right);

            if(right == null)
            {
                NotImplementedMethod(category, defineableToken, right, "contextSearchResult", contextSearchResult);
                return null;
            }

            var argType = Type(right);
            var prefixSearchResult = argType.SearchDefineablePrefix(defineableToken);
            if(prefixSearchResult.IsSuccessFull)
                return prefixSearchResult.Feature.ApplyResult(this, category, right);

            NotImplementedMethod(category, defineableToken, right, "contextSearchResult", contextSearchResult,
                                 "prefixSearchResult", prefixSearchResult);
            return null;
        }

        internal Result InfixResult(Category category, ICompileSyntax left, DefineableToken defineableToken,
                                    ICompileSyntax right)
        {
            var leftType = Type(left).EnsureRef(RefAlignParam);
            var searchResult = leftType.SearchDefineable(defineableToken);
            if(searchResult.IsSuccessFull)
                return searchResult.Feature.ApplyResult(this, category, left, right);
            NotImplementedMethod(category, left, defineableToken, right, "leftType", leftType, "searchResult",
                                 searchResult);
            return null;
        }

        internal Result Result(Category category, ICompileSyntax left, DefineableToken defineableToken,
                               ICompileSyntax right)
        {
            if(left == null)
                return PrefixResult(category, defineableToken, right);
            return InfixResult(category, left, defineableToken, right);
        }

        internal TypeBase CondBranchType(ICompileSyntax syntax)
        {
            if(syntax == null)
                return TypeBase.CreateVoid;

            return Type(syntax).AutomaticDereference();
        }

        public Result AsymetricCondBranchResult(Refs condRefs, Category category, ICompileSyntax syntax)
        {
            Tracer.Assert(!category.HasCode);

            if(syntax == null)
                return TypeBase.CreateVoid.CreateResult(category);

            var result = Result(category, syntax).AutomaticDereference().Clone();
            if(category.HasRefs && !condRefs.IsNone)
                result.Refs += condRefs;

            return result;
        }

        internal Result CondBranchResult(Category category, ICompileSyntax syntax, TypeBase commonType)
        {
            var branchResult = Result(category | Category.Type, syntax).AutomaticDereference();
            return branchResult.Type
                .Conversion(category | Category.Type, commonType)
                .UseWithArg(branchResult)
                .CreateStatement(category, RefAlignParam);
        }

        internal virtual Result PendingResult(Category category, ICompileSyntax syntax) { return CreatePendingResultContext().PendingResult(category, syntax); }
    }

    [Serializable]
    internal class Cache : ReniObject, IIconKeyProvider
    {
        [Node, SmartNode]
        internal readonly DictionaryEx<TypeBase, Function> _functionInstanceCache =
            new DictionaryEx<TypeBase, Function>();

        [Node, SmartNode]
        internal readonly DictionaryEx<ICompileSyntax, TypeBase> _functionType =
            new DictionaryEx<ICompileSyntax, TypeBase>();

        [Node, SmartNode]
        internal readonly DictionaryEx<ICompileSyntax, TypeBase> _propertyType =
            new DictionaryEx<ICompileSyntax, TypeBase>();

        [Node, SmartNode]
        internal readonly DictionaryEx<Container, FullContext> _structContainerCache =
            new DictionaryEx<Container, FullContext>();

        [Node, SmartNode]
        internal readonly SimpleCache<Result> _topRefResultCache = new SimpleCache<Result>();

        [Node, SmartNode]
        internal readonly DictionaryEx<ICompileSyntax, CacheItem> _resultCache =
            new DictionaryEx<ICompileSyntax, CacheItem>();

        [Node, SmartNode]
        internal readonly SimpleCache<PendingResultContext> _pendingResultContext =
            new SimpleCache<PendingResultContext>();

        /// <summary>
        /// Gets the icon key.
        /// </summary>
        /// <value>The icon key.</value>
        [DumpData(false)]
        public string IconKey { get { return "Cache"; } }
    }

    internal class PendingResultContext : Child
    {
        public PendingResultContext(ContextBase parent)
            : base(parent) { }

        internal override Result PendingResult(Category category, ICompileSyntax syntax)
        {
            return Result(category, syntax);
        }
    }
}