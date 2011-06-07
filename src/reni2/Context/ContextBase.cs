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
    internal sealed class ContextBase : ReniObject, IDumpShortProvider, IIconKeyProvider, IReferenceInCode
    {
        private static int _nextId;

        [IsDumpEnabled(false)]
        private readonly CacheItems _cache;

        [IsDumpEnabled(false)]
        private readonly ContextBase _parent;

        [IsDumpEnabled(false)]
        private readonly IContextItem _contextItem;

        private ContextBase(ContextBase parent, IContextItem contextItem)
            : base(_nextId++)
        {
            _parent = parent;
            _contextItem = contextItem;
            _cache = new CacheItems(this);
        }

        RefAlignParam IReferenceInCode.RefAlignParam { get { return RefAlignParam; } }
        bool IReferenceInCode.IsChildOf(ContextBase parentCandidate) { return ChildChain.StartsWithAndNotEqual(parentCandidate.ChildChain); }

        internal static ContextBase CreateRoot(FunctionList functions) { return new ContextBase(null, new Root(functions)); }

        [Node]
        [IsDumpEnabled(false)]
        [UsedImplicitly]
        internal CacheItems Cache { get { return _cache; } }

        [Node]
        [IsDumpEnabled(false)]
        internal RefAlignParam RefAlignParam { get { return _contextItem.RefAlignParam ?? Parent.RefAlignParam; } }

        [Node]
        internal ContextBase Parent { get { return _parent; } }

        [IsDumpEnabled(false)]
        internal int AlignBits { get { return RefAlignParam.AlignBits; } }

        [IsDumpEnabled(false)]
        internal Size RefSize { get { return RefAlignParam.RefSize; } }

        [IsDumpEnabled(false)]
        internal Root RootContext { get { return _contextItem as Root ?? _parent.RootContext; } }

        [IsDumpEnabled(false)]
        internal ContextBase[] ChildChain { get { return Cache.ChildChain ?? (Cache.ChildChain = ObtainChildChain()); } }

        [IsDumpEnabled(false)]
        internal StructContext FindRecentStructContext { get { return Cache.RecentStructContext.Value; } }

        private ContextBase[] ObtainChildChain()
        {
            if(Parent == null)
                return new[] {this};
            return Parent.ChildChain.Concat(new[] {this}).ToArray();
        }

        internal string DumpShort() { return base.ToString(); }

        [UsedImplicitly]
        internal int SizeToPacketCount(Size size) { return size.SizeToPacketCount(RefAlignParam.AlignBits); }

        internal ContextBase SpawnFunction(TypeBase args)
        {
            return _cache
                .FunctionInstances
                .Find(args);
        }

        private ContextBase SpawnPendingContext { get { return _cache.PendingContext.Value; } }

        internal ContextBase SpawnStruct(Struct.Container container, int position)
        {
            return _cache
                .StructContexts
                .Find(container)
                .Find(position);
        }

        internal IReferenceInCode SpawnStruct(Struct.Context context) { return SpawnStruct(context.Container, context.Position); }

        internal Result CreateArgsReferenceResult(Category category)
        {
            var result = _contextItem.CreateArgsReferenceResult(this, category);
            return result ?? Parent.CreateArgsReferenceResult(category);
        }

        internal void Search(SearchVisitor<IContextFeature> searchVisitor)
        {
            _contextItem.Search(searchVisitor);
            if(searchVisitor.IsSuccessFull)
                return;
            if(Parent != null)
                Parent.Search(searchVisitor);
            if(searchVisitor.IsSuccessFull)
                return;
            searchVisitor.SearchTypeBase();
        }

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

        private StructContext ObtainRecentStructContext()
        {
            var result = _contextItem as Struct.Context;
            if(result != null)
                return new StructContext(result, Parent);
            return Parent.ObtainRecentStructContext();
        }

        internal Result AtTokenResult(Category category, ICompileSyntax left, ICompileSyntax right)
        {
            var context = Type(left).GetStruct();
            var position = Evaluate(right, context.IndexType).ToInt32();
            var atResult = context.AccessResultFromArg(category, position);
            var leftResult = ResultAsRef(category, left);
            var result = atResult.ReplaceArg(leftResult);
            return result;
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

        internal Result PendingResult(Category category, ICompileSyntax syntax)
        {
            if(_contextItem is PendingContext)
            {
                var result = syntax.Result(this, category);
                Tracer.Assert(result.CompleteCategory == category);
                return result;
            }
            return SpawnPendingContext.PendingResult(category, syntax);
        }

        internal Result CommonResult(Category category, CondSyntax condSyntax)
        {
            if(!(_contextItem is PendingContext))
                return condSyntax.CommonResult(this, category);

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
            NotImplementedMethod(category, condSyntax);
            return null;
        }

        internal Category PendingCategory(ICompileSyntax syntax) { return _cache.ResultCache[syntax].Data.PendingCategory; }

        internal TypeBase CommonType(CondSyntax condSyntax) { return CommonResult(Category.Type, condSyntax).Type; }

        internal Refs CommonRefs(CondSyntax condSyntax) { return CommonResult(Category.Refs, condSyntax).Refs; }

        internal sealed class CacheItems : ReniObject, IIconKeyProvider
        {
            [IsDumpEnabled(false)]
            internal readonly SimpleCache<StructContext> RecentStructContext;

            [IsDumpEnabled(false)]
            internal ContextBase[] ChildChain;

            [Node]
            [SmartNode]
            internal readonly DictionaryEx<TypeBase, ContextBase> FunctionInstances;

            [Node]
            [SmartNode]
            internal readonly DictionaryEx<Struct.Container, DictionaryEx<int, ContextBase>> StructContexts;

            [Node]
            [SmartNode]
            internal readonly DictionaryEx<ICompileSyntax, CacheItem> ResultCache;

            [Node]
            [SmartNode]
            internal readonly SimpleCache<ContextBase> PendingContext;

            public CacheItems(ContextBase parent)
            {
                ResultCache = new DictionaryEx<ICompileSyntax, CacheItem>(parent.CreateCacheElement);
                StructContexts = new DictionaryEx<Struct.Container, DictionaryEx<int, ContextBase>>(
                    container => new DictionaryEx<int, ContextBase>(
                        position => new ContextBase(parent, container.SpawnContext(position))));
                FunctionInstances = new DictionaryEx<TypeBase, ContextBase>(args => new ContextBase(parent, new Function(args)));
                PendingContext = new SimpleCache<ContextBase>(() => new ContextBase(parent, new PendingContext()));
                RecentStructContext = new SimpleCache<StructContext>(parent.ObtainRecentStructContext);
            }

            /// <summary>
            ///     Gets the icon key.
            /// </summary>
            /// <value>The icon key.</value>
            [IsDumpEnabled(false)]
            public string IconKey { get { return "Cache"; } }
        }

    }

    internal sealed class ContextOperator : Defineable, ISearchPath<IFeature, FunctionAccessType>
    {
        IFeature ISearchPath<IFeature, FunctionAccessType>.Convert(FunctionAccessType type) { return new Feature.Feature(type.ContextOperatorFeatureApply); }
    }

    internal sealed class PendingContext : Child
    {}
}