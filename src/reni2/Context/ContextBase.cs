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
using Reni.Struct;
using Reni.Syntax;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Context
{
    /// <summary>
    ///     Base class for compiler environments
    /// </summary>
    [Serializable]
    internal abstract class ContextBase : ReniObject, IDumpShortProvider, IIconKeyProvider
    {
        private static int _nextId;

        [IsDumpEnabled(false)]
        private readonly Cache _cache;

        [Node, IsDumpEnabled(false), UsedImplicitly]
        internal Cache Cache { get { return _cache; } }

        private ContextBase[] _childChainCache;

        protected ContextBase()
            : base(_nextId++)
        {
            _cache = new Cache
                (
                this
                );
        }

        [Node, IsDumpEnabled(false)]
        internal abstract RefAlignParam RefAlignParam { get; }

        [IsDumpEnabled(false)]
        public int AlignBits { get { return RefAlignParam.AlignBits; } }

        [IsDumpEnabled(false)]
        public Size RefSize { get { return RefAlignParam.RefSize; } }

        [IsDumpEnabled(false)]
        internal abstract Root RootContext { get; }

        [IsDumpEnabled(false)]
        internal ContextBase[] ChildChain { get { return _childChainCache ?? (_childChainCache = ObtainChildChain()); } }

        protected virtual ContextBase[] ObtainChildChain() { return new[] {this}; }

        internal virtual string DumpShort() { return base.ToString(); }

        [UsedImplicitly]
        internal int SizeToPacketCount(Size size) { return size.SizeToPacketCount(RefAlignParam.AlignBits); }

        internal static Root CreateRoot() { return new Root(); }

        internal Function CreateFunction(TypeBase args)
        {
            return _cache
                .FunctionInstanceCache
                .Find(args);
        }

        private PendingContext CreatePendingContext() { return _cache.PendingContext.Value; }

        internal FullContext CreateStruct(Struct.Container container)
        {
            return _cache
                .StructContainerCache
                .Find(container);
        }

        internal virtual Result CreateArgsReferenceResult(Category category)
        {
            NotImplementedMethod(category);
            return null;
        }

        internal virtual void Search(SearchVisitor<IContextFeature> searchVisitor) { searchVisitor.SearchTypeBase(); }

        internal TypeBase Type(ICompileSyntax syntax)
        {
            var result = Result(Category.Type, syntax).Type;
            Tracer.Assert(result != null);
            return result;
        }

        [DebuggerHidden]
        internal Result Result(Category category, ICompileSyntax syntax)
        {
            return _cache
                .ResultCache
                .Find(syntax)
                .Result(category);
        }

        internal CacheItem CreateCacheElement(ICompileSyntax syntax)
        {
            var result = new CacheItem(syntax, this);
            syntax.AddToCacheForDebug(this, result);
            return result;
        }

        internal bool IsChildOf(ContextBase parentCandidate) { return ChildChain.StartsWithAndNotEqual(parentCandidate.ChildChain); }

        internal void AssertCorrectRefs(Result result)
        {
            if(result.HasRefs)
                AssertCorrectRefs(result.Refs.Data);
            else if(result.HasCode)
                AssertCorrectRefs(result.Code.RefsData);
        }

        private void AssertCorrectRefs(IEnumerable<IReferenceInCode> refs)
        {
            foreach(var @ref in refs)
                CheckRef(@ref);
        }

        private void CheckRef(IReferenceInCode reference) { Tracer.Assert(!reference.IsChildOf(this), () => "context=" + Dump() + "\nref=" + reference.Dump()); }

        internal BitsConst Evaluate(ICompileSyntax syntax, TypeBase resultType)
        {
            return Result(Category.Code | Category.Type | Category.Refs, syntax)
                .ConvertTo(resultType)
                .Evaluate();
        }

        internal BitsConst Evaluate(ICompileSyntax syntax)
        {
            return Result(Category.Code | Category.Type | Category.Refs, syntax)
                .Evaluate();
        }

        [UsedImplicitly]
        internal CodeBase Code(ICompileSyntax syntax) { return Result(Category.Code, syntax).Code; }

        internal Result ResultAsRef(Category category, ICompileSyntax syntax)
        {
            var result = Result(category | Category.Type, syntax);
            if(result.Type.IsRef(RefAlignParam) || result.SmartSize.IsZero)
                return Result(category, syntax);

            return result.LocalReferenceResult(RefAlignParam);
        }

        internal Result ConvertedRefResult(Category category, ICompileSyntax syntax, Reference target)
        {
            var result = Result(category | Category.Type, syntax);
            return result.ConvertToAsRef(category, target);
        }

        string IDumpShortProvider.DumpShort() { return DumpShort(); }

        /// <summary>
        ///     Gets the icon key.
        /// </summary>
        /// <value>The icon key.</value>
        string IIconKeyProvider.IconKey { get { return "Context"; } }

        internal virtual Struct.Context FindStruct()
        {
            NotImplementedMethod();
            return null;
        }

        internal Result Result(Category category, ICompileSyntax left, Defineable defineable, ICompileSyntax right)
        {
            var trace = defineable.ObjectId == -20 && category.HasCode;
            StartMethodDump(trace, category, left, defineable, right);
            var categoryForFunctionals = category;
            if(right != null)
                categoryForFunctionals |= Category.Type;

            if(left == null && right != null)
            {
                var prefixResult = Type(right)
                    .PrefixResult(category, defineable);
                if(prefixResult != null)
                    return prefixResult.ReplaceArg(Result(category | Category.Type, right));
            }

            var suffixResult =
                left == null
                    ? ContextResult(categoryForFunctionals, defineable)
                    : SuffixResult(categoryForFunctionals, left, defineable);

            if(right == null)
                return ReturnMethodDumpWithBreak(trace, suffixResult);

            var suffixType = suffixResult.Type;
            DumpWithBreak(trace, "suffixResult", suffixResult);
            var result = suffixType.Apply(category, rightCategory => ResultAsRef(rightCategory, right), RefAlignParam);
            DumpWithBreak(trace, "result", result);
            return ReturnMethodDumpWithBreak(trace, result.ReplaceArg(suffixResult));
        }

        private Result SuffixResult(Category category, ICompileSyntax left, Defineable defineable)
        {
            var trace = ObjectId == -5 && defineable.ObjectId == 4 && category.HasCode;
            StartMethodDumpWithBreak(trace, category, left, defineable);
            var suffixResult = Type(left)
                .GetSuffixResult(category, defineable);
            if(suffixResult == null)
            {
                NotImplementedMethod(category, left, defineable);
                return null;
            }

            var leftResult = Result(category, left);
            DumpWithBreak(trace, "suffixResult", suffixResult, "leftResult", leftResult);
            var result = suffixResult.ReplaceArg(leftResult);
            return ReturnMethodDumpWithBreak(trace, result);
        }

        private IContextFeature SearchDefinable(Defineable defineable)
        {
            var visitor = new ContextSearchVisitor(defineable);
            visitor.Search(this);
            return visitor.Result;
        }

        private Result ContextResult(Category category, Defineable defineable)
        {
            var feature = SearchDefinable(defineable);
            if(feature == null)
            {
                NotImplementedMethod(category, defineable);
                return null;
            }

            return feature.Apply(category) & category;
        }

        internal virtual Result PendingResult(Category category, ICompileSyntax syntax) { return CreatePendingContext().PendingResult(category, syntax); }

        internal virtual Result CommonResult(Category category, CondSyntax condSyntax) { return condSyntax.CommonResult(this, category); }

        internal Category PendingCategory(ICompileSyntax syntax) { return _cache.ResultCache[syntax].Data.PendingCategory; }

        internal TypeBase CommonType(CondSyntax condSyntax) { return CommonResult(Category.Type, condSyntax).Type; }

        internal Refs CommonRefs(CondSyntax condSyntax) { return CommonResult(Category.Refs, condSyntax).Refs; }
    }

    [Serializable]
    internal sealed class Cache : ReniObject, IIconKeyProvider
    {
        [Node, SmartNode]
        internal readonly DictionaryEx<TypeBase, Function> FunctionInstanceCache;

        [Node, SmartNode]
        internal readonly DictionaryEx<Struct.Container, FullContext> StructContainerCache;

        [Node, SmartNode]
        internal readonly DictionaryEx<ICompileSyntax, CacheItem> ResultCache;

        [Node, SmartNode]
        internal readonly SimpleCache<PendingContext> PendingContext;

        public Cache(ContextBase contextBase)
        {
            ResultCache = new DictionaryEx<ICompileSyntax, CacheItem>(contextBase.CreateCacheElement);
            StructContainerCache = new DictionaryEx<Struct.Container, FullContext>(container => new FullContext(contextBase, container));
            FunctionInstanceCache = new DictionaryEx<TypeBase, Function>(args => new Function(contextBase, args));
            PendingContext = new SimpleCache<PendingContext>(() => new PendingContext(contextBase)
                );
        }

        /// <summary>
        ///     Gets the icon key.
        /// </summary>
        /// <value>The icon key.</value>
        [IsDumpEnabled(false)]
        public string IconKey { get { return "Cache"; } }
    }

    internal class PendingContext : Child
    {
        public PendingContext(ContextBase parent)
            : base(parent) { }

        internal override Result PendingResult(Category category, ICompileSyntax syntax)
        {
            var result = syntax.Result(this, category);
            Tracer.Assert(result.CompleteCategory == category);
            return result;
        }

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

    internal class ContextOperator : Defineable, ISearchPath<IFeature, FunctionAccessType>
    {
        IFeature ISearchPath<IFeature, FunctionAccessType>.Convert(FunctionAccessType type) { return new Feature.Feature(type.ContextOperatorFeatureApply); }
    }
}