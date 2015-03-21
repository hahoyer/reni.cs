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
using Reni.Parser;
using Reni.ReniParser;
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
        protected override string GetNodeDump() => base.GetNodeDump() + "(" + GetContextIdentificationDump() + ")";

        static int _nextId;

        [DisableDump]
        [Node]
        readonly Cache _cache;

        protected ContextBase()
            : base(_nextId++) { _cache = new Cache(this); }

        public abstract string GetContextIdentificationDump();

        string IIconKeyProvider.IconKey => "Context";

        [DisableDump]
        [Node]
        internal abstract Root RootContext { get; }

        [DisableDump]
        internal CompoundView FindRecentCompoundView => _cache.RecentStructure.Value;

        [DisableDump]
        internal IFunctionContext FindRecentFunctionContextObject => _cache.RecentFunctionContextObject.Value;

        [UsedImplicitly]
        internal int SizeToPacketCount(Size size) => size.SizeToPacketCount(Root.DefaultRefAlignParam.AlignBits);

        internal ContextBase CompoundPositionContext(CompoundSyntax container, int position)
            => _cache.CompoundContexts[container][position];

        internal CompoundView CompoundView(CompoundSyntax syntax, int? accessPosition = null)
            => _cache.CompoundViews[syntax][accessPosition ?? syntax.EndPosition];

        internal Compound Compound(CompoundSyntax context) => _cache.Compounds[context];

        [DebuggerHidden]
        internal Result Result(Category category, CompileSyntax syntax)
        {
            var cacheItem = ResultCache(syntax);
            cacheItem.Update(category);
            var result = cacheItem.Data & category;

            var pendingCategory = category - result.CompleteCategory;
            if(pendingCategory.HasAny)
            {
                var pendingResult = syntax.PendingResultForCache(this, pendingCategory);
                Tracer.Assert(pendingCategory <= pendingResult.CompleteCategory);
                result.Update(pendingResult);
            }
            Tracer.Assert(category == result.CompleteCategory);
            return result;
        }

        internal ResultCache ResultCache(CompileSyntax syntax) => _cache.ResultCache[syntax];

        internal TypeBase TypeIfKnown(CompileSyntax syntax) => _cache.ResultCache[syntax].Data.Type;

        [DebuggerHidden]
        Result ResultForCache(Category category, CompileSyntax syntax)
        {
            var trace = syntax.ObjectId == -23 && ObjectId == 1 && category.HasCode;
            StartMethodDump(trace, category, syntax);
            try
            {
                BreakExecution();
                var result = syntax.ResultForCache(this, category.Replenished);
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
            var result = new ResultCache(category => ResultForCache(category, syntax));
            syntax.AddToCacheForDebug(this, result);
            return result;
        }

        internal TypeBase Type(CompileSyntax syntax) => Result(Category.Type, syntax).Type;

        internal virtual CompoundView ObtainRecentCompoundView() => null;
        internal virtual IFunctionContext ObtainRecentFunctionContext() => null;

        sealed class Cache : DumpableObject, IIconKeyProvider
        {
            [Node]
            [DisableDump]
            internal readonly ValueCache<CompoundView> RecentStructure;

            [Node]
            [DisableDump]
            internal readonly ValueCache<IFunctionContext> RecentFunctionContextObject;

            [Node]
            [SmartNode]
            internal readonly FunctionCache<CompoundSyntax, FunctionCache<int, ContextBase>> CompoundContexts;

            [Node]
            [SmartNode]
            internal readonly FunctionCache<CompoundSyntax, FunctionCache<int, CompoundView>> CompoundViews;

            [Node]
            [SmartNode]
            internal readonly FunctionCache<CompoundSyntax, Compound> Compounds;

            [Node]
            [SmartNode]
            internal readonly FunctionCache<CompileSyntax, ResultCache> ResultCache;

            public Cache(ContextBase target)
            {
                ResultCache = new FunctionCache<CompileSyntax, ResultCache>(target.CreateCacheElement);
                CompoundContexts = new FunctionCache<CompoundSyntax, FunctionCache<int, ContextBase>>
                    (
                    container =>
                        new FunctionCache<int, ContextBase>
                            (position => new CompoundViewContext(target, target.CompoundView(container, position)))
                    );
                RecentStructure = new ValueCache<CompoundView>(target.ObtainRecentCompoundView);
                RecentFunctionContextObject = new ValueCache<IFunctionContext>(target.ObtainRecentFunctionContext);
                CompoundViews = new FunctionCache<CompoundSyntax, FunctionCache<int, CompoundView>>
                    (
                    container =>
                        new FunctionCache<int, CompoundView>
                            (position => new CompoundView(Compounds[container], position))
                    );
                Compounds = new FunctionCache<CompoundSyntax, Compound>
                    (container => new Compound(container, target));
            }

            [DisableDump]
            public string IconKey => "Cache";
        }

        internal Result ResultAsReference(Category category, CompileSyntax syntax)
            => Result(category.Typed, syntax)
                .LocalReferenceResult;

        internal Result ArgReferenceResult(Category category)
            => FindRecentFunctionContextObject
                .CreateArgReferenceResult(category);

        internal Result ArgsResult(Category category, [CanBeNull] CompileSyntax right)
            => right == null
                ? RootContext.VoidType.Result(category.Typed)
                : right.SmartUnFunctionedReferenceResult(this, category);

        /// <summary>
        ///     Obtains the feature result of a functional argument object.
        ///     Actual arguments, if provided, as well as object reference are replaced.
        /// </summary>
        /// <param name="category"> the categories in result </param>
        /// <param name="right"> the expression of the argument of the call. Must not be null </param>
        /// <returns> </returns>
        internal Result FunctionalArgResult(Category category, CompileSyntax right)
        {
            var argsType = FindRecentFunctionContextObject.ArgsType;
            var functionalArgDescriptor = new ContextSearchResult(argsType.CheckedFeature, RootContext);
            return functionalArgDescriptor.Execute(category, argsType.FindRecentCompoundView.ObjectPointerViaContext, this, right);
        }

        ContextSearchResult Declarations(Definable tokenClass)
        {
            var genericize = tokenClass.Genericize.ToArray();
            var results = genericize.SelectMany(g => g.Declarations(this));
            var result = results.SingleOrDefault();
            if(result != null || RootContext.ProcessErrors)
                return result;
            NotImplementedMethod(tokenClass);
            return null;
        }

        internal Result PrefixResult(Category category, Definable definable, ExpressionSyntax expression)
        {
            var searchResult = Declarations(definable);
            if(searchResult == null)
                return RootContext.UndefinedSymbol(expression).Result(category);

            var result = searchResult.Execute(category, FindRecentCompoundView.ObjectPointerViaContext, this, expression.Right);
            Tracer.Assert(category <= result.CompleteCategory);
            return result;
        }

        internal virtual IEnumerable<ContextSearchResult> Declarations<TDefinable>(TDefinable tokenClass)
            where TDefinable : Definable
        {
            var provider = this as ISymbolProviderForPointer<TDefinable, IFeatureImplementation>;
            var feature = provider?.Feature(tokenClass);
            if(feature != null)
                yield return new 
                    ContextSearchResult(feature, RootContext);
        }
        public IssueType UndefinedSymbol(ExpressionSyntax source)
            =>
                new IssueType
                    (
                    new Issue(IssueId.UndefinedSymbol, source, "Context: " + Dump()),
                    RootContext);
    }
}