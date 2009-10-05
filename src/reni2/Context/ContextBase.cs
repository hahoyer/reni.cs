using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.TreeStructure;
using JetBrains.Annotations;
using Reni.Code;
using Reni.Feature;
using Reni.Parser.TokenClass;
using Reni.Struct;
using Reni.Syntax;
using Reni.Type;

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
        private readonly Cache _cache;

        private ContextBase[] _childChainCache;

        protected ContextBase()
            : base(_nextId++)
        {
            _cache = new Cache
                (
                () => new PendingContext(this)
                );
        }

        [Node, DumpData(false)]
        internal abstract RefAlignParam RefAlignParam { get; }

        [DumpData(false)]
        public int AlignBits { get { return RefAlignParam.AlignBits; } }

        [DumpData(false)]
        public Size RefSize { get { return RefAlignParam.RefSize; } }

        [DumpData(false)]
        internal abstract Root RootContext { get; }

        [DumpData(false)]
        internal ContextBase[] ChildChain
        {
            get
            {
                if(_childChainCache == null)
                    _childChainCache = ObtainChildChain();
                return _childChainCache;
            }
        }

        protected virtual ContextBase[] ObtainChildChain() { return new []{this}; }

        internal virtual string DumpShort() { return base.ToString(); }

        [UsedImplicitly]
        internal int SizeToPacketCount(Size size) { return size.SizeToPacketCount(RefAlignParam.AlignBits); }

        internal static Root CreateRoot() { return new Root(); }

        internal Function CreateFunction(TypeBase args)
        {
            return _cache.FunctionInstanceCache.Find(args,
                                                     () => new Function(this, args));
        }

        private PendingContext CreatePendingContext() { return _cache.PendingContext.Value; }

        internal FullContext CreateStruct(Struct.Container container)
        {
            return _cache.StructContainerCache.Find(container,
                                                    () => new FullContext(this, container));
        }

        internal virtual Result CreateArgsRefResult(Category category)
        {
            NotImplementedMethod(category);
            return null;
        }

        internal Result CreateFunctionResult(Category category, ICompileSyntax body) { return CreateFunctionType(body).CreateResult(category); }

        private TypeBase CreateFunctionType(ICompileSyntax body) { return _cache.FunctionType.Find(body, () => new Type.Function(this, body)); }

        internal virtual void Search(SearchVisitor<IContextFeature> searchVisitor)
        {
            searchVisitor.SearchTypeBase();
        }

        internal TypeBase Type(ICompileSyntax syntax)
        {
            var result = Result(Category.Type, syntax).Type;
            Tracer.Assert(result != null);
            return result;
        }

        //[DebuggerHidden]
        internal Result Result(Category category, ICompileSyntax syntax)
        {
            return _cache.ResultCache.Find
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

        internal bool IsChildOf(ContextBase parentCandidate)
        {
            return ChildChain.StartsWithAndNotEqual(parentCandidate.ChildChain);
        }

        internal void AssertCorrectRefs(Result result)
        {
            if(result.HasRefs)
                AssertCorrectRefs(result.Refs.Data);
            else if(result.HasCode)
                AssertCorrectRefs(result.Code.RefsData);
        }

        private void AssertCorrectRefs(IEnumerable<IRefInCode> refs)
        {
            foreach(var @ref in refs)
                CheckRef(@ref);
        }

        private void CheckRef(IRefInCode @ref)
        {
            Tracer.Assert(!@ref.IsChildOf(this), "context=" + Dump() + "\nref="+ @ref.Dump());
        }

        internal BitsConst Evaluate(ICompileSyntax syntax, TypeBase resultType) { return Result(Category.Code | Category.Type | Category.Refs, syntax).ConvertTo(resultType).Evaluate(); }

        internal BitsConst Evaluate(ICompileSyntax syntax) { return Result(Category.Code | Category.Type | Category.Refs, syntax).Evaluate(); }

        [UsedImplicitly]
        internal CodeBase Code(ICompileSyntax syntax) { return Result(Category.Code, syntax).Code; }

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

        internal Result GetResult(Category category, ICompileSyntax left, Defineable defineable, ICompileSyntax right)
        {
            var categoryForFunctionals = category;
            if (right != null)
                categoryForFunctionals |= Category.Type;

            var suffixResult = GetSuffixResult(categoryForFunctionals, left, defineable);
            if(suffixResult == null)
            {
                if(left != null)
                {
                    NotImplementedMethod(category, left, defineable, right);
                    return null;
                }

                var prefixResult = GetPrefixResult(category, defineable, right);
                if (prefixResult != null)
                    return prefixResult;
                suffixResult = GetContextResult(categoryForFunctionals, defineable);
            }
            if(right == null)
                return suffixResult;

            var feature = suffixResult.Type.GetFunctionalFeature();
            if (feature != null)
                return feature.Apply(category, suffixResult, ResultAsRef(category|Category.Type, right));
            NotImplementedMethod(category, left, defineable, right, "suffixResult", suffixResult,"feature",feature);
            return null;
        }

        private IContextFeature SearchDefinable(Defineable defineable)
        {
            var visitor = new ContextSearchVisitor(defineable);
            visitor.Search(this);
            return visitor.Result;
        }

        private Result GetPrefixResult(Category category, Defineable defineable, ICompileSyntax right)
        {
            return GetUnaryResult<IPrefixFeature>(category, right, defineable);
        }

        private Result GetSuffixResult(Category category, ICompileSyntax left, Defineable defineable)
        {
            return GetUnaryResult<IFeature>(category, left, defineable);
        }

        private Result GetUnaryResult<TFeature>(Category category, ICompileSyntax left, Defineable defineable)
            where TFeature : class
        {
            if (left == null)
                return null;
            var leftType = Type(left).EnsureRef(RefAlignParam);

            var rawResult = leftType.GetUnaryResult<TFeature>(category, defineable);
            if(rawResult == null)
                return null;

            var result = rawResult.UseWithArg(ResultAsRef(category, left));
            return result;
        }

        private Result GetContextResult(Category category, Defineable defineable)
        {
            IContextFeature feature = SearchDefinable(defineable);
            if (feature == null)
                return null;

            return feature.Apply(category) & category;
        }

        internal virtual Result PendingResult(Category category, ICompileSyntax syntax) { return CreatePendingContext().PendingResult(category, syntax); }

        internal virtual Result CommonResult(Category category, CondSyntax condSyntax) { return condSyntax.CommonResult(this, category); }

        internal Category PendingCategory(ICompileSyntax syntax) { return _cache.ResultCache[syntax].Data.PendingCategory; }

        internal TypeBase CommonType(CondSyntax condSyntax) { return CommonResult(Category.Type, condSyntax).Type; }

        internal Refs CommonRefs(CondSyntax condSyntax) { return CommonResult(Category.Refs, condSyntax).Refs; }
    }

    [Serializable]
    internal class Cache : ReniObject, IIconKeyProvider
    {
        [Node, SmartNode]
        internal readonly DictionaryEx<TypeBase, Function> FunctionInstanceCache =
            new DictionaryEx<TypeBase, Function>();

        [Node, SmartNode]
        internal readonly DictionaryEx<ICompileSyntax, TypeBase> FunctionType =
            new DictionaryEx<ICompileSyntax, TypeBase>();

        [Node, SmartNode]
        internal readonly DictionaryEx<Struct.Container, FullContext> StructContainerCache =
            new DictionaryEx<Struct.Container, FullContext>();

        [Node, SmartNode]
        internal readonly DictionaryEx<ICompileSyntax, CacheItem> ResultCache =
            new DictionaryEx<ICompileSyntax, CacheItem>();

        [Node, SmartNode]
        internal readonly SimpleCache<PendingContext> PendingContext;

        public Cache(Func<PendingContext> pendingContext) { PendingContext = new SimpleCache<PendingContext>(pendingContext); }

        /// <summary>
        /// Gets the icon key.
        /// </summary>
        /// <value>The icon key.</value>
        [DumpData(false)]
        public string IconKey { get { return "Cache"; } }
    }

    internal class PendingContext : Child
    {
        public PendingContext(ContextBase parent)
            : base(parent) { }

        internal override Result PendingResult(Category category, ICompileSyntax syntax) { return syntax.Result(this, category); }

        internal override Result CommonResult(Category category, CondSyntax condSyntax)
        {
            if(category <= Parent.PendingCategory(condSyntax))
            {
                return condSyntax.CommonResult
                    (
                    this,
                    category,
                    category <= Parent.PendingCategory(condSyntax.Then),
                    condSyntax.Else != null && category <= Parent.PendingCategory(condSyntax.Else)
                    );
            }
            return base.CommonResult(category, condSyntax);
        }
    }
}