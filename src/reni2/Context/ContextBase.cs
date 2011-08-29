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
    internal abstract partial class ContextBase : ReniObject, IDumpShortProvider, IIconKeyProvider
    {
        private static int _nextId;

        [DisableDump]
        [Node]
        private readonly CacheItems _cache;

        protected ContextBase()
            : base(_nextId++) { _cache = new CacheItems(this); }

        string IDumpShortProvider.DumpShort() { return DumpShort(); }

        string IIconKeyProvider.IconKey { get { return "Context"; } }

        [Node]
        [DisableDump]
        internal CacheItems Cache { get { return _cache; } }

        [Node]
        [DisableDump]
        internal RefAlignParam RefAlignParam { get { return Root.DefaultRefAlignParam; } }

        [DisableDump]
        internal int AlignBits { get { return RefAlignParam.AlignBits; } }

        [DisableDump]
        internal abstract Root RootContext { get; }

        [DisableDump]
        internal Structure FindRecentStructure { get { return Cache.RecentStructure.Value; } }

        [DisableDump]
        internal FunctionContextObject FindRecentFunctionContextObject { get { return Cache.RecentFunctionContextObject.Value; } }

        [UsedImplicitly]
        internal int SizeToPacketCount(Size size) { return size.SizeToPacketCount(RefAlignParam.AlignBits); }

        internal FunctionContextObject UniqueFunctionContextObject(TypeBase args) { return _cache.FunctionContextObjects.Find(args); }
        internal ContextBase UniqueChildContext(Container container, int position) { return _cache.StructContexts.Find(container).Find(position); }
        internal ContextBase UniqueChildContext(TypeBase args) { return _cache.FunctionContexts.Find(args); }
        private PendingContext UniquePendingContext { get { return _cache.PendingContext.Value; } }
        internal Structure UniqueStructure(Struct.Context context) { return Cache.Structures.Find(context.Container).Find(context.Position); }
        internal Structure UniqueStructure(Container container) { return UniqueStructure(container, container.EndPosition); }
        internal Structure UniqueStructure(Container container, int accessPosition) { return Cache.Structures.Find(container).Find(accessPosition); }
        internal ContainerContextObject UniqueContainerContext(Container context) { return Cache.ContainerContextObjects.Find(context); }

        internal virtual void Search(SearchVisitor<IContextFeature> searchVisitor) { searchVisitor.SearchTypeBase(); }

        [DebuggerHidden]
        internal Result UniqueResult(Category category, CompileSyntax syntax)
        {
            var cacheItem = _cache.ResultCache.Find(syntax);
            cacheItem.Update(category);
            var result = cacheItem.Data & category;
            Tracer.Assert(category == result.CompleteCategory);
            return result;
        }

        internal Result QuickResult(Category category, CompileSyntax syntax) { return _cache.ResultCache.Find(syntax).Data & category; }

        [DebuggerHidden]
        private Result ObtainResult(Category category, CompileSyntax syntax)
        {
            var trace = syntax.ObjectId == -44 && category.HasIsDataLess;
            StartMethodDump(trace, category, syntax);
            try
            {
                BreakExecution();
                return ReturnMethodDump(syntax.ObtainResult(this, category.Replenished), true);
            }
            finally
            {
                EndMethodDump();
            }
        }

        private CacheItem CreateCacheElement(CompileSyntax syntax)
        {
            var result = new CacheItem(syntax, this);
            syntax.AddToCacheForDebug(this, result);
            return result;
        }

        private IContextFeature SearchDefinable(Defineable defineable)
        {
            var visitor = new ContextSearchVisitor(defineable);
            visitor.Search(this);
            return visitor.Result;
        }

        internal Result ContextOperationResult(Category category, Defineable defineable)
        {
            var feature = SearchDefinable(defineable);
            if(feature == null)
            {
                NotImplementedMethod(category, defineable);
                return null;
            }

            return feature.ObtainResult(category) & category;
        }

        protected virtual Result PendingResult(Category category, CompileSyntax syntax) { return UniquePendingContext.Result(category, syntax); }
        protected abstract Result CommonResult(Category category, CondSyntax condSyntax);
        internal virtual Structure ObtainRecentStructure() { return null; }
        internal virtual FunctionContextObject ObtainRecentFunctionContext() { return null; }

        internal Category PendingCategory(CompileSyntax syntax) { return _cache.ResultCache[syntax].Data.PendingCategory; }

        internal TypeBase CommonType(CondSyntax condSyntax) { return CommonResult(Category.Type, condSyntax).Type; }

        internal CodeArgs CommonRefs(CondSyntax condSyntax) { return CommonResult(Category.Args, condSyntax).CodeArgs; }

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
            internal readonly DictionaryEx<TypeBase, Function> FunctionContexts;

            [Node]
            [SmartNode]
            internal readonly DictionaryEx<CompileSyntax, CacheItem> ResultCache;

            [Node]
            [SmartNode]
            internal readonly SimpleCache<PendingContext> PendingContext;


            public CacheItems(ContextBase target)
            {
                ResultCache = new DictionaryEx<CompileSyntax, CacheItem>(target.CreateCacheElement);
                StructContexts = new DictionaryEx<Container, DictionaryEx<int, ContextBase>>(
                    container => new DictionaryEx<int, ContextBase>(
                                     position => new Struct.Context(target, container, position)));
                FunctionContexts = new DictionaryEx<TypeBase, Function>(argsType => new Function(target, argsType));
                FunctionContextObjects = new DictionaryEx<TypeBase, FunctionContextObject>(args => new FunctionContextObject(args, target));
                PendingContext = new SimpleCache<PendingContext>(() => new PendingContext(target));
                RecentStructure = new SimpleCache<Structure>(target.ObtainRecentStructure);
                RecentFunctionContextObject = new SimpleCache<FunctionContextObject>(target.ObtainRecentFunctionContext);
                Structures = new DictionaryEx<Container, DictionaryEx<int, Structure>>(
                    container => new DictionaryEx<int, Structure>(
                                     position => new Structure(ContainerContextObjects.Find(container), position)));
                ContainerContextObjects = new DictionaryEx<Container, ContainerContextObject>(container => new ContainerContextObject(container, target));
            }

            [DisableDump]
            public string IconKey { get { return "Cache"; } }
        }
    }
}