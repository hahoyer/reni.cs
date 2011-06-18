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

        [DisableDump]
        private readonly CacheItems _cache;

        [DisableDump]
        private readonly ContextBase _parent;

        [DisableDump]
        private readonly IContextItem _contextItem;

        private ContextBase(ContextBase parent, IContextItem contextItem)
            : base(_nextId++)
        {
            _parent = parent;
            _contextItem = contextItem;
            _cache = new CacheItems(this);
        }

        string IDumpShortProvider.DumpShort() { return DumpShort(); }

        string IIconKeyProvider.IconKey { get { return "Context"; } }

        RefAlignParam IReferenceInCode.RefAlignParam { get { return RefAlignParam; } }
        bool IReferenceInCode.IsChildOf(ContextBase parentCandidate) { return IsChildOf(parentCandidate); }
        internal static ContextBase CreateRoot(FunctionList functions) { return new ContextBase(null, new Root(functions)); }

        [UsedImplicitly]
        internal IContextItem ContextItem { get { return _contextItem; } }
        [UsedImplicitly]
        internal ContextBase Parent { get { return _parent; } }

        [Node]
        [DisableDump]
        [UsedImplicitly]
        internal CacheItems Cache { get { return _cache; } }

        [Node]
        [DisableDump]
        internal RefAlignParam RefAlignParam { get { return ContextItem.RefAlignParam ?? Parent.RefAlignParam; } }

        [DisableDump]
        internal int AlignBits { get { return RefAlignParam.AlignBits; } }

        [DisableDump]
        internal Root RootContext { get { return ContextItem as Root ?? Parent.RootContext; } }

        [DisableDump]
        private ContextBase[] ChildChain { get { return Cache.ChildChain ?? (Cache.ChildChain = ObtainChildChain()); } }

        [DisableDump]
        internal AccessPoint FindRecentAccessPoint { get { return Cache.RecentAccessPoint.Value; } }

        [DisableDump]
        private FunctionContextObject FindRecentFunctionContextObject { get { return Cache.RecentFunctionContextObject.Value; } }

        private ContextBase[] ObtainChildChain()
        {
            if(Parent == null)
                return new[] {this};
            return Parent.ChildChain.Concat(new[] {this}).ToArray();
        }

        [UsedImplicitly]
        internal int SizeToPacketCount(Size size) { return size.SizeToPacketCount(RefAlignParam.AlignBits); }

        internal bool IsChildOf(ContextBase parentCandidate) { return ChildChain.StartsWithAndNotEqual(parentCandidate.ChildChain); }
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

        internal Result CreateArgsReferenceResult(Category category)
        {
            return FindRecentFunctionContextObject.CreateArgsReferenceResult(category);
        }

        internal void Search(SearchVisitor<IContextFeature> searchVisitor)
        {
            ContextItem.Search(searchVisitor, Parent);
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

        private CacheItem CreateCacheElement(ICompileSyntax syntax)
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

        private AccessPoint ObtainRecentStructContext()
        {
            var result = ContextItem as Struct.Context;
            if(result != null)
                return result.Container.SpawnContainerContext(Parent).SpawnAccessPoint(result.Position);
            return Parent.ObtainRecentStructContext();
        }

        private FunctionContextObject ObtainRecentFunctionContext()
        {
            var result = ContextItem as Function;
            if (result != null)
                return result.SpawnFunctionContext(Parent);
            if (Parent == null)
                return null;
            return Parent.ObtainRecentFunctionContext();
        }

        internal Result AtTokenResult(Category category, ICompileSyntax left, ICompileSyntax right)
        {
            var leftResultAsRef = ResultAsRef(category | Category.Type, left);
            var rightResult = Result(Category.All, right);
            return leftResultAsRef
                .Type
                .GetStructAccessPoint()
                .Access(category, leftResultAsRef, rightResult);
        }

        internal Result Result(Category category, ICompileSyntax left, Defineable defineable, ICompileSyntax right)
        {
            var trace = defineable.ObjectId == 18 && category.HasCode;
            StartMethodDump(trace, category, left, defineable, right);
            var categoryForFunctionals = category;
            if(right != null)
                categoryForFunctionals |= Category.Type;

            if(left == null && right != null)
            {
                var prefixOperationResult = Type(right)
                    .PrefixOperationResult(category, defineable);
                if(prefixOperationResult != null)
                    return prefixOperationResult.ReplaceArg(Result(category | Category.Type, right));
            }

            var suffixOperationResult =
                left == null
                    ? ContextOperationResult(categoryForFunctionals, defineable)
                    : SuffixOperationResult(categoryForFunctionals, left, defineable);

            if(right == null)
                return ReturnMethodDumpWithBreak(trace, suffixOperationResult);

            var suffixOperationType = suffixOperationResult.Type;
            var rightResult = ResultAsRef(categoryForFunctionals, right);
            DumpWithBreak(trace, "suffixOperationResult", suffixOperationResult, "rightResult", rightResult);
            var result = suffixOperationType.Apply(category, rightResult, RefAlignParam);
            DumpWithBreak(trace, "result", result);
            return ReturnMethodDumpWithBreak(trace, result.ReplaceArg(suffixOperationResult));
        }

        private Result SuffixOperationResult(Category category, ICompileSyntax left, Defineable defineable)
        {
            var trace = ObjectId == -4 && defineable.ObjectId == 25 && category.HasCode;
            StartMethodDumpWithBreak(trace, category, left, defineable);
            var suffixResult = Type(left)
                .SuffixResult(category, defineable);
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

        private Result ContextOperationResult(Category category, Defineable defineable)
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
            if(ContextItem is PendingContext)
            {
                var result = syntax.Result(this, category);
                Tracer.Assert(result.CompleteCategory == category);
                return result;
            }
            return SpawnPendingContext.PendingResult(category, syntax);
        }

        private Result CommonResult(Category category, CondSyntax condSyntax)
        {
            if(!(ContextItem is PendingContext))
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

        private Category PendingCategory(ICompileSyntax syntax) { return _cache.ResultCache[syntax].Data.PendingCategory; }

        internal TypeBase CommonType(CondSyntax condSyntax) { return CommonResult(Category.Type, condSyntax).Type; }

        internal Refs CommonRefs(CondSyntax condSyntax) { return CommonResult(Category.Refs, condSyntax).Refs; }

        internal sealed class CacheItems : ReniObject, IIconKeyProvider
        {
            [DisableDump]
            internal readonly SimpleCache<AccessPoint> RecentAccessPoint;

            [DisableDump]
            internal readonly SimpleCache<FunctionContextObject> RecentFunctionContextObject;

            [DisableDump]
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
                RecentAccessPoint = new SimpleCache<AccessPoint>(parent.ObtainRecentStructContext);
                RecentFunctionContextObject = new SimpleCache<FunctionContextObject>(parent.ObtainRecentFunctionContext);
            }

            /// <summary>
            ///     Gets the icon key.
            /// </summary>
            /// <value>The icon key.</value>
            [DisableDump]
            public string IconKey { get { return "Cache"; } }
        }

    }

    internal sealed class ContextOperator : Defineable, ISearchPath<IFeature, TypeBase>
    {
        IFeature ISearchPath<IFeature, TypeBase>.Convert(TypeBase type)
        {
            return new Feature.Feature(type.ContextOperatorFeatureApply);
        }
    }

    internal sealed class PendingContext : Child
    {}
}