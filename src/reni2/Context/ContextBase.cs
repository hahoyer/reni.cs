using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using hw.Debug;
using hw.Forms;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using JetBrains.Annotations;
using Reni.Basics;
using Reni.Feature;
using Reni.ReniSyntax;
using Reni.Struct;
using Reni.TokenClasses;
using Reni.Type;
using Reni.Validation;

namespace Reni.Context
{
    /// <summary>
    ///     Base class for compiler environments
    /// </summary>
    abstract class ContextBase
        : DumpableObject
            , IIconKeyProvider
    {
        static int _nextId;

        [DisableDump]
        [Node]
        readonly Cache _cache;

        protected ContextBase()
            : base(_nextId++)
        {
            _cache = new Cache(this);
        }

        string IIconKeyProvider.IconKey { get { return "Context"; } }

        [DisableDump]
        [Node]
        internal abstract Root RootContext { get; }

        [DisableDump]
        internal Structure FindRecentStructure { get { return _cache.RecentStructure.Value; } }

        [DisableDump]
        internal IFunctionContext FindRecentFunctionContextObject { get { return _cache.RecentFunctionContextObject.Value; } }
        public abstract string DumpPrintText { get; }

        [UsedImplicitly]
        internal int SizeToPacketCount(Size size)
        {
            return size.SizeToPacketCount(Root.DefaultRefAlignParam.AlignBits);
        }

        internal ContextBase UniqueStructurePositionContext(Container container, int position)
        {
            return _cache.StructContexts[container][position];
        }
        internal Structure UniqueStructure(Container container) { return UniqueStructure(container, container.EndPosition); }
        internal Structure UniqueStructure(Container container, int accessPosition)
        {
            return _cache.Structures[container][accessPosition];
        }
        internal ContainerContextObject UniqueContainerContext(Container context)
        {
            return _cache.ContainerContextObjects[context];
        }

        [DebuggerHidden]
        internal Result Result(Category category, CompileSyntax syntax)
        {
            var cacheItem = _cache.ResultCache[syntax];
            cacheItem.Update(category);
            var result = cacheItem.Data & category;

            var pendingCategory = category - result.CompleteCategory;
            if(pendingCategory.HasAny)
            {
                var pendingResult = syntax.ObtainPendingResult(this, pendingCategory);
                Tracer.Assert(pendingCategory <= pendingResult.CompleteCategory);
                result.Update(pendingResult);
            }
            Tracer.Assert(category == result.CompleteCategory);
            return result;
        }

        internal TypeBase TypeIfKnown(CompileSyntax syntax) { return _cache.ResultCache[syntax].Data.Type; }

        [DebuggerHidden]
        Result ObtainResult(Category category, CompileSyntax syntax)
        {
            var trace = syntax.ObjectId == -23 && ObjectId == 1 && category.HasCode;
            StartMethodDump(trace, category, syntax);
            try
            {
                BreakExecution();
                var result = syntax.ObtainResult(this, category.Replenished);
                Tracer.Assert(category <= result.CompleteCategory);
                return ReturnMethodDump(result);
            }
            finally
            {
                EndMethodDump();
            }
        }

        [DebuggerHidden]
        ResultCache CreateCacheElement(CompileSyntax syntax)
        {
            var result = new ResultCache(category => ObtainResult(category, syntax));
            syntax.AddToCacheForDebug(this, result);
            return result;
        }

        internal TypeBase Type(CompileSyntax syntax) { return Result(Category.Type, syntax).Type; }

        internal virtual Structure ObtainRecentStructure() { return null; }
        internal virtual IFunctionContext ObtainRecentFunctionContext() { return null; }

        internal virtual bool? QuickHllw(CompileSyntax compileSyntax) { return null; }

        sealed class Cache : DumpableObject, IIconKeyProvider
        {
            [Node]
            [DisableDump]
            internal readonly ValueCache<Structure> RecentStructure;

            [Node]
            [DisableDump]
            internal readonly ValueCache<IFunctionContext> RecentFunctionContextObject;

            [Node]
            [SmartNode]
            internal readonly FunctionCache<Container, FunctionCache<int, ContextBase>> StructContexts;

            [Node]
            [SmartNode]
            internal readonly FunctionCache<Container, FunctionCache<int, Structure>> Structures;

            [Node]
            [SmartNode]
            internal readonly FunctionCache<Container, ContainerContextObject> ContainerContextObjects;

            [Node]
            [SmartNode]
            internal readonly FunctionCache<CompileSyntax, ResultCache> ResultCache;

            [Node]
            [SmartNode]
            internal readonly FunctionCache<SourcePart, IssueType> UndefinedSymbolType;

            public Cache(ContextBase target)
            {
                UndefinedSymbolType = new FunctionCache<SourcePart, IssueType>
                    (tokenData => UndefinedSymbolIssue.Type(tokenData, target));
                ResultCache = new FunctionCache<CompileSyntax, ResultCache>(target.CreateCacheElement);
                StructContexts = new FunctionCache<Container, FunctionCache<int, ContextBase>>
                    (
                    container =>
                        new FunctionCache<int, ContextBase>
                            (position => new Struct.Context(target, container, position))
                    );
                RecentStructure = new ValueCache<Structure>(target.ObtainRecentStructure);
                RecentFunctionContextObject = new ValueCache<IFunctionContext>(target.ObtainRecentFunctionContext);
                Structures = new FunctionCache<Container, FunctionCache<int, Structure>>
                    (
                    container =>
                        new FunctionCache<int, Structure>
                            (position => new Structure(ContainerContextObjects[container], position))
                    );
                ContainerContextObjects = new FunctionCache<Container, ContainerContextObject>
                    (container => new ContainerContextObject(container, target));
            }

            [DisableDump]
            public string IconKey { get { return "Cache"; } }
        }

        internal Result ResultAsReference(Category category, CompileSyntax syntax)
        {
            return Result(category.Typed, syntax)
                .LocalPointerKindResult;
        }

        internal Result ArgReferenceResult(Category category)
        {
            return FindRecentFunctionContextObject
                .CreateArgReferenceResult(category);
        }
        internal Result ArgsResult(Category category, [CanBeNull]CompileSyntax right)
        {
            return right == null
                ? RootContext.VoidType.Result(category.Typed)
                : right.SmartUnFunctionedReferenceResult(this, category);
        }

        internal Result ObjectResult(Category category, [NotNull] CompileSyntax left)
        {
            return Result(category.Typed, left)
                .Conversion(Type(left).TypeForSearchProbes);
        }

        /// <summary>
        ///     Obtains the feature result of a functional argument object.
        ///     Actual arguments, if provided, as well as object reference are replaced.
        /// </summary>
        /// <param name="category"> the categories in result </param>
        /// <param name="right"> the expression of the argument of the call. Must not be null </param>
        /// <returns> </returns>
        internal Result FunctionalArgResult(Category category, CompileSyntax right)
        {
            var functionalArgDescriptor = new FunctionalArgDescriptor(this);
            return functionalArgDescriptor.Result(category, this, right);
        }

        /// <summary>
        ///     Obtains the feature result of a functional object.
        ///     Actual arguments, if provided, as well as object reference are replaced.
        /// </summary>
        /// <param name="category"> the categories in result </param>
        /// <param name="left"> the expression left to the feature access, if provided </param>
        /// <param name="right"> the expression right to the feature access, if provided </param>
        /// <returns> </returns>
        internal Result FunctionalObjectResult(Category category, [NotNull] CompileSyntax left, CompileSyntax right)
        {
            var functionalObjectDescriptor = new FunctionalObjectDescriptor(this, left);
            return functionalObjectDescriptor.Result(category, this, left, right);
        }

        ContextSearchResult Declarations(Definable tokenClass)
        {
            var genericize = tokenClass.Genericize.ToArray();
            var result = genericize
                .SelectMany(g => g.Declarations(this))
                .SingleOrDefault();
            if(RootContext.ProcessErrors)
                return result;
            NotImplementedMethod(tokenClass);
            return null;

        }

        internal Result ObtainResult(Category category, SourcePart position, Definable tokenClass, CompileSyntax right)
        {
            var searchResult = Declarations(tokenClass);
            if(searchResult == null)
                return UndefinedSymbolIssue.Type(position, RootContext).IssueResult(category);

            var result = searchResult.CallResult(this, category, right);
            Tracer.Assert(category <= result.CompleteCategory);
            return result;
        }

        virtual internal IEnumerable<ContextSearchResult> Declarations<TDefinable>(TDefinable tokenClass) where TDefinable : Definable
        {
            var provider = this as ISymbolProvider<TDefinable, IFeatureImplementation>;
            if(provider != null)
                yield return new ContextSearchResult(provider.Feature(tokenClass), this);
        }
        internal Result CreateArrayResult(Category category, CompileSyntax argsType)
        {
            var target = Result(category.Typed, argsType).Align;
            return target.Type.UniqueAlign.UniqueArray(1).Result(category, target);
        }
    }
}