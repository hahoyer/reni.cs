//     Compiler for programming language "Reni"
//     Copyright (C) 2011 Harald Hoyer
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//     
//     Comments, bugs and suggestions to hahoyer at yahoo.de

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.TreeStructure;
using JetBrains.Annotations;
using Reni.Basics;
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
    internal sealed class ContextBase : ReniObject, IDumpShortProvider, IIconKeyProvider
    {
        private static int _nextId;

        [DisableDump]
        [Node]
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
        [Node]
        internal IContextItem ContextItem { get { return _contextItem; } }
        [UsedImplicitly]
        [Node]
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

        internal ContextBase UniqueChildContext(Container container, int position)
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

        internal Result CreateArgsReferenceResult(Category category) { return FindRecentFunctionContextObject.CreateArgsReferenceResult(category); }

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

        internal Result ObtainResult(Category category, ICompileSyntax syntax)
        {
            StartMethodDump(ObjectId == -1 && syntax.GetObjectId() == 25 && (category.HasType || category.HasCode), category, syntax);
            try
            {
                return ReturnMethodDump(syntax.Result(this, category), true);
            }
            finally
            {
                EndMethodDump();
            }
        }
        private ContextBase UniquePendingContext { get { return _cache.PendingContext.Value; } }

        private CacheItem CreateCacheElement(ICompileSyntax syntax)
        {
            var result = new CacheItem(syntax, this);
            syntax.AddToCacheForDebug(this, result);
            return result;
        }

        internal Result ResultAsReference(Category category, ICompileSyntax syntax) { return Result(category.Typed, syntax).LocalReferenceResult(RefAlignParam); }

        private Structure ObtainRecentStructure()
        {
            var result = ContextItem as Struct.Context;
            if(result != null)
                return Parent.UniqueStructure(result);
            return Parent.ObtainRecentStructure();
        }

        internal Structure UniqueStructure(Struct.Context context) { return Cache.Structures.Find(context.Container).Find(context.Position); }
        internal Structure UniqueStructure(Container container) { return Cache.Structures.Find(container).Find(container.EndPosition); }
        internal ContainerContextObject UniqueContainerContext(Container context) { return Cache.ContainerContextObjects.Find(context); }

        private FunctionContextObject ObtainRecentFunctionContext()
        {
            var result = ContextItem as Function;
            if(result != null)
                return Parent.UniqueFunction(result.ArgsType);
            if(Parent == null)
                return null;
            return Parent.ObtainRecentFunctionContext();
        }

        internal Result AtTokenResult(Category category, ICompileSyntax left, ICompileSyntax right)
        {
            var leftResultAsRef = ResultAsReference(category | Category.Type, left);
            var rightResult = Result(Category.All, right);
            return leftResultAsRef
                .Type
                .FindRecentStructure
                .AccessViaThisReference(category, rightResult)
                .ReplaceArg(leftResultAsRef);
        }

        internal Result CallResult(Category category, ICompileSyntax left, ICompileSyntax right)
        {
            var leftResult = Result(category.Typed, left);
            var rightResult = Result(category.Typed, right);
            return leftResult
                .Type
                .FunctionalFeature
                .ObtainApplyResult(category, leftResult, rightResult, RefAlignParam);
        }

        internal Result Result(Category category, ICompileSyntax left, Defineable defineable, ICompileSyntax right)
        {
            var trace = defineable.ObjectId == -12 && right != null && right.GetObjectId() == 24 && (category.HasCode || category.HasType);
            StartMethodDump(trace, category, left, defineable, right);
            try
            {
                BreakExecution();
                var categoryForFunctionals = category;
                if(right != null)
                    categoryForFunctionals |= Category.Type;

                if(left == null && right != null)
                {
                    var prefixOperationResult = OperationResult<IPrefixFeature>(category, right, defineable);
                    if(prefixOperationResult != null)
                        return ReturnMethodDump(prefixOperationResult);
                }

                var suffixOperationResult =
                    left == null
                        ? ContextOperationResult(categoryForFunctionals, defineable)
                        : OperationResult<IFeature>(categoryForFunctionals, left, defineable);

                if(suffixOperationResult == null)
                {
                    NotImplementedMethod(category, left, defineable, right);
                    return null;
                }

                if(right == null)
                    return ReturnMethodDump(suffixOperationResult, true);

                var functionalFeature = suffixOperationResult.Type.FunctionalFeature;
                var rightCategory = functionalFeature.IsRegular ? category.Typed : Category.All;
                var rightResult = Result(rightCategory, right).LocalReferenceResult(RefAlignParam);
                Dump("suffixOperationResult", suffixOperationResult);
                Dump("rightResult", rightResult);
                BreakExecution();
                var result = functionalFeature
                    .ObtainApplyResult(category, suffixOperationResult, rightResult, RefAlignParam);
                return ReturnMethodDump(result, true);
            }
            finally
            {
                EndMethodDump();
            }
        }

        private Result OperationResult<TFeature>(Category category, ICompileSyntax target, Defineable defineable)
            where TFeature : class
        {
            var trace = defineable.ObjectId == -12 && target.GetObjectId() == 24 && (category.HasCode || category.HasType);
            StartMethodDump(trace, category, target, defineable);
            try
            {
                BreakExecution();
                var targetType = Type(target);
                Dump("targetType", targetType);
                BreakExecution();
                var operationResult = targetType.OperationResult<TFeature>(category, defineable, RefAlignParam);
                Dump("operationResult", operationResult);
                BreakExecution();
                if(operationResult == null)
                    return ReturnMethodDump<Result>(null);

                var targetResult = ResultAsReference(category.Typed, target);
                Dump("targetResult", targetResult);
                BreakExecution();
                var result = operationResult.ReplaceArg(targetResult);
                return ReturnMethodDump(result, true);
            }
            finally
            {
                EndMethodDump();
            }
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

            return feature.ObtainResult(category) & category;
        }

        internal Result PendingResult(Category category, ICompileSyntax syntax)
        {
            if(ContextItem is PendingContext)
            {
                if(category == Category.Refs)
                    return new Result(category, Refs.ArgLess);
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

            var pendingCategory = Parent.PendingCategory(condSyntax);
            if(category <= pendingCategory)
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
            [Node]
            [DisableDump]
            internal readonly SimpleCache<Structure> RecentStructure;

            [Node]
            [DisableDump]
            internal readonly SimpleCache<FunctionContextObject> RecentFunctionContextObject;

            [Node]
            [SmartNode]
            internal readonly DictionaryEx<TypeBase, FunctionContextObject> FunctionContextObjects;

            [Node]
            [SmartNode]
            internal readonly DictionaryEx<Container, DictionaryEx<int, ContextBase>> StructContexts;

            [Node]
            [SmartNode]
            internal readonly DictionaryEx<Container, DictionaryEx<int, Structure>> Structures;

            [Node]
            [SmartNode]
            internal readonly DictionaryEx<Container, ContainerContextObject> ContainerContextObjects;

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
                StructContexts = new DictionaryEx<Container, DictionaryEx<int, ContextBase>>(
                    container => new DictionaryEx<int, ContextBase>(
                                     position => new ContextBase(parent, container.UniqueContext(position))));
                FunctionContexts = new DictionaryEx<TypeBase, ContextBase>(argsType => new ContextBase(parent, argsType.UniqueFunction()));
                FunctionContextObjects = new DictionaryEx<TypeBase, FunctionContextObject>(args => new FunctionContextObject(args, parent));
                PendingContext = new SimpleCache<ContextBase>(() => new ContextBase(parent, new PendingContext()));
                RecentStructure = new SimpleCache<Structure>(parent.ObtainRecentStructure);
                RecentFunctionContextObject = new SimpleCache<FunctionContextObject>(parent.ObtainRecentFunctionContext);
                Structures = new DictionaryEx<Container, DictionaryEx<int, Structure>>(
                    container => new DictionaryEx<int, Structure>(
                                     position => new Structure(ContainerContextObjects.Find(container), position)));
                ContainerContextObjects = new DictionaryEx<Container, ContainerContextObject>(container => new ContainerContextObject(container, parent));
            }

            [DisableDump]
            public string IconKey { get { return "Cache"; } }
        }

    }

    internal sealed class PendingContext : Child
    {}
}