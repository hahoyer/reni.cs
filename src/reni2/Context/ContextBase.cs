using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.TreeStructure;
using JetBrains.Annotations;
using Reni.Basics;
using Reni.Code;
using Reni.Feature;
using Reni.Parser;
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
    internal sealed class ContextBase : ReniObject, IDumpShortProvider, IIconKeyProvider
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
        internal Structure FindRecentStructure { get { return Cache.RecentStructure.Value; } }

        [DisableDump]
        internal FunctionContextObject FindRecentFunctionContextObject { get { return Cache.RecentFunctionContextObject.Value; } }

        [UsedImplicitly]
        internal int SizeToPacketCount(Size size) { return size.SizeToPacketCount(RefAlignParam.AlignBits); }

        internal FunctionContextObject UniqueFunction(TypeBase args)
        {
            return _cache
                .FunctionContextObjects
                .Find(args);
        }

        internal ContextBase UniqueChildContext(Struct.Container container, int position)
        {
            return _cache
                .StructContexts
                .Find(container)
                .Find(position);
        }

        internal ContextBase UniqueChildContext(TypeBase args)
        {
            return _cache
                .FunctionContexts
                .Find(args);
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

        internal CodeBase Code(ICompileSyntax syntax) { return Result(Category.Code, syntax).Code; }

        internal BitsConst Evaluate(ICompileSyntax syntax, TypeBase resultType)
        {
            return Result(Category.Code | Category.Type | Category.Refs, syntax)
                .Conversion(resultType)
                .Evaluate();
        }

        internal BitsConst Evaluate(ICompileSyntax syntax)
        {
            return Result(Category.Code | Category.Type | Category.Refs, syntax)
                .Evaluate();
        }

        private ContextBase[] ObtainChildChain()
        {
            if (Parent == null)
                return new[] { this };
            return Parent.ChildChain.Concat(new[] { this }).ToArray();
        }

        private ContextBase UniquePendingContext { get { return _cache.PendingContext.Value; } }

        private CacheItem CreateCacheElement(ICompileSyntax syntax)
        {
            var result = new CacheItem(syntax, this);
            syntax.AddToCacheForDebug(this, result);
            return result;
        }

        internal Result ResultAsReference(Category category, ICompileSyntax syntax)
        {
            var result = Result(category | Category.Type, syntax);
            if(result.Type.IsRef(RefAlignParam) || result.SmartSize.IsZero)
                return Result(category, syntax);

            return result.LocalReferenceResult(RefAlignParam);
        }

        private Structure ObtainRecentStructure()
        {
            var result = ContextItem as Struct.Context;
            if (result != null)
                return Parent.UniqueStructure(result);
            return Parent.ObtainRecentStructure();
        }

        internal Structure UniqueStructure(Struct.Context context) { return Cache.Structures.Find(context.Container).Find(context.Position); }
        internal Structure UniqueStructure(Struct.Container container) { return Cache.Structures.Find(container).Find(container.EndPosition); }
        internal ContainerContextObject UniqueContainerContext(Struct.Container context) { return Cache.ContainerContextObjects.Find(context); }

        private FunctionContextObject ObtainRecentFunctionContext()
        {
            var result = ContextItem as Function;
            if (result != null)
                return Parent.UniqueFunction(result.ArgsType);
            if (Parent == null)
                return null;
            return Parent.ObtainRecentFunctionContext();
        }

        internal Result AtTokenResult(Category category, ICompileSyntax left, ICompileSyntax right)
        {
            var leftResultAsRef = ResultAsReference(category | Category.Type, left);
            var rightResult = Result(Category.All, right);
            return leftResultAsRef
                .Type
                .GetStructure()
                .AccessViaThisReference(category, rightResult)
                .ReplaceArg(leftResultAsRef);
        }

        internal Result Result(Category category, ICompileSyntax left, Defineable defineable, ICompileSyntax right)
        {
            var trace = defineable.ObjectId == -18 && category.HasCode;
            StartMethodDump(trace, category, left, defineable, right);
            var categoryForFunctionals = category;
            if(right != null)
                categoryForFunctionals |= Category.Type;

            if(left == null && right != null)
            {
                var prefixOperationResult = OperationResult<IPrefixFeature>(category, right, defineable);
                if(prefixOperationResult != null)
                    return prefixOperationResult;
            }

            var suffixOperationResult =
                left == null
                    ? ContextOperationResult(categoryForFunctionals, defineable)
                    : OperationResult<IFeature>(categoryForFunctionals, left, defineable);

            if (suffixOperationResult == null)
            {
                NotImplementedMethod(category, left, defineable, right);
                return null;
            }

            if (right == null)
                return ReturnMethodDumpWithBreak(trace, suffixOperationResult);

            var rightResult = ResultAsReference(categoryForFunctionals, right);
            DumpWithBreak(trace, "suffixOperationResult", suffixOperationResult, "rightResult", rightResult);
            var result = suffixOperationResult
                .Type
                .FunctionalFeature.Apply(category, suffixOperationResult, rightResult, RefAlignParam);
            return ReturnMethodDumpWithBreak(trace, result);
        }

        private Result OperationResult<TFeature>(Category category, ICompileSyntax target, Defineable defineable) 
            where TFeature : class
        {
            var trace = defineable.ObjectId == -18 && category.HasCode;
            StartMethodDumpWithBreak(trace, category, target, defineable);
            var targetType = Type(target);
            DumpWithBreak(trace, "targetType", targetType);
            var operationResult = targetType.OperationResult<TFeature>(category, defineable, RefAlignParam);
            if(operationResult == null)
                return null;

            DumpWithBreak(trace, "operationResult", operationResult);
            var targetResult = ResultAsReference(category | Category.Type, target);
            DumpWithBreak(trace, "targetResult", targetResult);
            var result = operationResult.ReplaceArg(targetResult);
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
            return UniquePendingContext.PendingResult(category, syntax);
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
            internal readonly SimpleCache<Structure> RecentStructure;

            [DisableDump]
            internal readonly SimpleCache<FunctionContextObject> RecentFunctionContextObject;

            [DisableDump]
            internal ContextBase[] ChildChain;

            [Node]
            [SmartNode]
            internal readonly DictionaryEx<TypeBase, FunctionContextObject> FunctionContextObjects;

            [Node]
            [SmartNode]
            internal readonly DictionaryEx<Struct.Container, DictionaryEx<int, ContextBase>> StructContexts;

            [Node]
            [SmartNode]
            internal readonly DictionaryEx<Struct.Container, DictionaryEx<int, Structure>> Structures;

            [Node]
            [SmartNode]
            internal readonly DictionaryEx<Struct.Container, ContainerContextObject> ContainerContextObjects;

            [Node]
            [SmartNode]
            internal readonly DictionaryEx<TypeBase, ContextBase> FunctionContexts;
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
                        position => new ContextBase(parent, container.UniqueContext(position))));
                FunctionContexts = new DictionaryEx<TypeBase, ContextBase>(argsType => new ContextBase(parent, argsType.UniqueFunction()));
                FunctionContextObjects = new DictionaryEx<TypeBase, FunctionContextObject>(args => new FunctionContextObject(args, parent));
                PendingContext = new SimpleCache<ContextBase>(() => new ContextBase(parent, new PendingContext()));
                RecentStructure = new SimpleCache<Structure>(parent.ObtainRecentStructure);
                RecentFunctionContextObject = new SimpleCache<FunctionContextObject>(parent.ObtainRecentFunctionContext);
                Structures = new DictionaryEx<Struct.Container, DictionaryEx<int, Structure>>(
                    container => new DictionaryEx<int, Structure>(
                        position => new Structure(ContainerContextObjects.Find(container),position)));
                ContainerContextObjects = new DictionaryEx<Struct.Container, ContainerContextObject>(container => new ContainerContextObject(container, parent));
            }

            [DisableDump]
            public string IconKey { get { return "Cache"; } }
        }

    }

    internal interface IReference
    {
        TypeBase ValueType{ get; }
        RefAlignParam RefAlignParam { get; }
    }

    internal sealed class ContextOperator : NonPrefix
    {
        public override Result Result(ContextBase context, Category category, TokenData token)
        {
            return context
                .FindRecentStructure
                .StructReferenceViaContextReference(category);
        }

        public override Result Result(ContextBase context, Category category, ICompileSyntax left)
        {
            return context
                .Type(left)
                .ThisReferenceResult(category)
                .ReplaceArg(context.Result(category, left));
        }
    }

    internal sealed class PendingContext : Child
    {}
}