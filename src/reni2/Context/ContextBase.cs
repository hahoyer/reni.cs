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
    abstract class ContextBase : ReniObject, IDumpShortProvider, IIconKeyProvider
    {
        static int _nextId;

        [DisableDump]
        [Node]
        readonly Cache _cache;

        protected ContextBase()
            : base(_nextId++) { _cache = new Cache(this); }

        string IDumpShortProvider.DumpShort() { return DumpShort(); }

        string IIconKeyProvider.IconKey { get { return "Context"; } }

        [Node]
        [DisableDump]
        [DebuggerHidden]
        internal RefAlignParam RefAlignParam { get { return Root.DefaultRefAlignParam; } }

        [DisableDump]
        internal int AlignBits { get { return RefAlignParam.AlignBits; } }

        [DisableDump]
        internal abstract Root RootContext { get; }

        [DisableDump]
        internal Structure FindRecentStructure { get { return _cache.RecentStructure.Value; } }

        [DisableDump]
        internal Function FindRecentFunctionContextObject { get { return _cache.RecentFunctionContextObject.Value; } }

        [UsedImplicitly]
        internal int SizeToPacketCount(Size size) { return size.SizeToPacketCount(RefAlignParam.AlignBits); }

        internal ContextBase UniqueChildContext(Container container, int position) { return _cache.StructContexts.Find(container).Find(position); }
        internal ContextBase UniqueChildContext(TypeBase args) { return _cache.FunctionContexts.Find(args); }
        PendingContext UniquePendingContext { get { return _cache.PendingContext.Value; } }
        internal Structure UniqueStructure(Container container) { return UniqueStructure(container, container.EndPosition); }
        internal Structure UniqueStructure(Container container, int accessPosition) { return _cache.Structures.Find(container).Find(accessPosition); }
        internal ContainerContextObject UniqueContainerContext(Container context) { return _cache.ContainerContextObjects.Find(context); }

        internal virtual void Search(SearchVisitor<IContextFeature> searchVisitor) { searchVisitor.SearchTypeBase(); }

        //[DebuggerHidden]
        internal Result UniqueResult(Category category, CompileSyntax syntax)
        {
            var cacheItem = _cache.ResultCache.Find(syntax);
            cacheItem.Update(category);
            var result = cacheItem.Data & category;
            Tracer.Assert(category == result.CompleteCategory);
            return result;
        }

        internal Result FindResult(Category category, CompileSyntax syntax) { return _cache.ResultCache.Find(syntax).Data & category; }

        //[DebuggerHidden]
        Result ObtainResult(Category category, CompileSyntax syntax)
        {
            var trace = syntax.ObjectId == -244 && ObjectId == 3 && category.HasIsDataLess;
            StartMethodDump(trace, category, syntax);
            try
            {
                BreakExecution();
                var result = syntax.ObtainResult(this, category.Replenished);
                Tracer.Assert(category <= result.CompleteCategory);
                return ReturnMethodDump(result, true);
            }
            finally
            {
                EndMethodDump();
            }
        }

        [DebuggerHidden]
        ResultCache CreateCacheElement(CompileSyntax syntax)
        {
            var result = new ResultCache((category, isPending) => ObtainResult(category, isPending, syntax));
            syntax.AddToCacheForDebug(this, result);
            return result;
        }

        //[DebuggerHidden]
        Result ObtainResult(Category category, bool isPending, CompileSyntax syntax)
        {
            if(isPending)
                return ObtainPendingResult(category, syntax);
            return ObtainResult(category, syntax);
        }

        SearchResult<IContextFeature> SearchDefinable(Defineable defineable)
        {
            var visitor = new ContextSearchVisitor(defineable);
            visitor.Search(this);
            return visitor.SearchResult;
        }

        internal Result ContextOperationResult(Category category, Defineable defineable)
        {
            var feature = SearchDefinable(defineable);
            if(feature == null)
            {
                NotImplementedMethod(category, defineable);
                return null;
            }
            return feature.ObtainObjectResult(category) & category;
        }

        protected virtual Result ObtainPendingResult(Category category, CompileSyntax syntax) { return UniquePendingContext.Result(category, syntax); }
        protected abstract Result CommonResult(Category category, CondSyntax condSyntax);
        internal virtual Structure ObtainRecentStructure() { return null; }
        internal virtual Function ObtainRecentFunctionContext() { return null; }

        internal Category PendingCategory(CompileSyntax syntax) { return _cache.ResultCache[syntax].Data.PendingCategory; }

        internal TypeBase CommonType(CondSyntax condSyntax) { return CommonResult(Category.Type, condSyntax).Type; }

        internal CodeArgs CommonRefs(CondSyntax condSyntax) { return CommonResult(Category.CodeArgs, condSyntax).CodeArgs; }

        internal virtual bool? QuickIsDataLess(CompileSyntax compileSyntax) { return null; }

        sealed class Cache : ReniObject, IIconKeyProvider
        {
            [Node]
            [DisableDump]
            internal readonly SimpleCache<Structure> RecentStructure;

            [Node]
            [DisableDump]
            internal readonly SimpleCache<Function> RecentFunctionContextObject;

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
            internal readonly DictionaryEx<TypeBase, Function> FunctionContexts;

            [Node]
            [SmartNode]
            internal readonly DictionaryEx<CompileSyntax, ResultCache> ResultCache;

            [Node]
            [SmartNode]
            internal readonly SimpleCache<PendingContext> PendingContext;


            public Cache(ContextBase target)
            {
                ResultCache = new DictionaryEx<CompileSyntax, ResultCache>(target.CreateCacheElement);
                StructContexts = new DictionaryEx<Container, DictionaryEx<int, ContextBase>>(
                    container => new DictionaryEx<int, ContextBase>(
                                     position => new Struct.Context(target, container, position)));
                FunctionContexts = new DictionaryEx<TypeBase, Function>(argsType => new Function(target, argsType));
                PendingContext = new SimpleCache<PendingContext>(() => new PendingContext(target));
                RecentStructure = new SimpleCache<Structure>(target.ObtainRecentStructure);
                RecentFunctionContextObject = new SimpleCache<Function>(target.ObtainRecentFunctionContext);
                Structures = new DictionaryEx<Container, DictionaryEx<int, Structure>>(
                    container => new DictionaryEx<int, Structure>(
                                     position => new Structure(ContainerContextObjects.Find(container), position)));
                ContainerContextObjects = new DictionaryEx<Container, ContainerContextObject>(container => new ContainerContextObject(container, target));
            }

            [DisableDump]
            public string IconKey { get { return "Cache"; } }
        }

        internal Result FunctionalResult(Category category, CompileSyntax target, bool isAutoCall)
        {
            return FindRecentStructure
                .UniqueFunctionalFeature(target)
                .Result(category, isAutoCall);
        }
        
        internal Result ObjectResult(Category category, CompileSyntax syntax)
        {
            if(syntax == null)
                return ObjectResult(category);

            return UniqueResult(category.Typed, syntax).LocalReferenceResult(RefAlignParam);
        }

        protected abstract Result ObjectResult(Category category);
    }
}