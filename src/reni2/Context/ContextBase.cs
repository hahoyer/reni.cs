using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using hw.Debug;
using hw.Forms;
using hw.Helper;
using hw.Scanner;
using JetBrains.Annotations;
using Reni.Basics;
using Reni.Feature;
using Reni.Parser;
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
        protected override string GetNodeDump()
            => base.GetNodeDump() + "(" + GetContextIdentificationDump() + ")";

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
        internal IFunctionContext FindRecentFunctionContextObject
            => _cache.RecentFunctionContextObject.Value;

        [DisableDump]
        internal abstract bool IsRecursionMode { get; }

        [UsedImplicitly]
        internal int SizeToPacketCount(Size size)
            => size.SizeToPacketCount(Root.DefaultRefAlignParam.AlignBits);

        internal ContextBase CompoundPositionContext(CompoundSyntax container, int position)
            => _cache.CompoundContexts[container][position];

        internal CompoundView CompoundView(CompoundSyntax syntax, int? accessPosition = null)
            => _cache.CompoundViews[syntax][accessPosition ?? syntax.EndPosition];

        internal Compound Compound(CompoundSyntax context) => _cache.Compounds[context];

        //[DebuggerHidden]
        internal Result Result(Category category, CompileSyntax syntax)
            => ResultCache(syntax).GetCategories(category);

        ResultCache ResultCache(CompileSyntax syntax) => _cache.ResultCache[syntax];

        internal ResultCache ResultAsReferenceCache(CompileSyntax syntax)
            => _cache.ResultAsReferenceCache[syntax];


        internal TypeBase TypeIfKnown(CompileSyntax syntax) => _cache.ResultCache[syntax].Data.Type;

        //[DebuggerHidden]
        Result ResultForCache(Category category, CompileSyntax syntax)
        {
            var trace = syntax.ObjectId == -959 && ObjectId == 39 && category.HasType;
            StartMethodDump(trace, category, syntax);
            try
            {
                BreakExecution();
                var result = syntax.ResultForCache(this, category.Replenished);
                Tracer.Assert(result == null || category <= result.CompleteCategory);
                return ReturnMethodDump(result);
            }
            finally
            {
                EndMethodDump();
            }
        }

        sealed class ResultProvider : ResultCache.IResultProvider
        {
            [EnableDumpExcept(false)]
            readonly bool AsReference;
            readonly ContextBase Context;
            readonly CompileSyntax Syntax;

            internal ResultProvider
                (ContextBase context, CompileSyntax syntax, bool asReference = false)
            {
                Context = context;
                Syntax = syntax;
                AsReference = asReference;
            }

            Result ResultCache.IResultProvider.Execute(Category category, Category pendingCategory)
            {
                if(pendingCategory.IsNone)
                    return AsReference
                        ? Context.ResultAsReference(category, Syntax)
                        : Context.ResultForCache(category, Syntax);

                var recursionHandler = Syntax.RecursionHandler;
                Tracer.Assert(recursionHandler != null);
                return recursionHandler.Execute(Context, category, pendingCategory, Syntax, AsReference);
            }


            [EnableDump]
            string ContextId => Context.NodeDump;

            [EnableDump]
            int SyntaxObjectId => Syntax.ObjectId;

            [EnableDump]
            string SyntaxText => Syntax.SourcePart.Id;

            object ResultCache.IResultProvider.Target => this;
        }

        [DebuggerHidden]
        ResultCache ResultCacheForCache(CompileSyntax syntax)
        {
            var result = new ResultCache(new ResultProvider(this, syntax));
            syntax.AddToCacheForDebug(this, result);
            return result;
        }

        ResultCache GetResultAsReferenceCacheForCache(CompileSyntax syntax)
            => new ResultCache(new ResultProvider(this, syntax, true));

        internal virtual CompoundView ObtainRecentCompoundView()
        {
            NotImplementedMethod();
            return null;
        }

        internal virtual IFunctionContext ObtainRecentFunctionContext()
        {
            NotImplementedMethod();
            return null;
        }

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
            internal readonly FunctionCache<CompoundSyntax, FunctionCache<int, ContextBase>>
                CompoundContexts;

            [Node]
            [SmartNode]
            internal readonly FunctionCache<CompoundSyntax, FunctionCache<int, CompoundView>>
                CompoundViews;

            [Node]
            [SmartNode]
            internal readonly FunctionCache<CompoundSyntax, Compound> Compounds;

            [Node]
            [SmartNode]
            internal readonly FunctionCache<CompileSyntax, ResultCache> ResultCache;

            [Node]
            [SmartNode]
            internal readonly FunctionCache<CompileSyntax, ResultCache> ResultAsReferenceCache;

            public Cache(ContextBase target)
            {
                ResultCache = new FunctionCache<CompileSyntax, ResultCache>
                    (target.ResultCacheForCache);
                ResultAsReferenceCache = new FunctionCache<CompileSyntax, ResultCache>
                    (target.GetResultAsReferenceCacheForCache);
                CompoundContexts = new FunctionCache
                    <CompoundSyntax, FunctionCache<int, ContextBase>>
                    (
                    container =>
                        new FunctionCache<int, ContextBase>
                            (
                            position =>
                                new CompoundViewContext
                                    (target, target.CompoundView(container, position)))
                    );
                RecentStructure = new ValueCache<CompoundView>(target.ObtainRecentCompoundView);
                RecentFunctionContextObject = new ValueCache<IFunctionContext>
                    (target.ObtainRecentFunctionContext);
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
            var functionalArgDescriptor = new ContextSearchResult
                (argsType.CheckedFeature, RootContext);
            return functionalArgDescriptor.Execute
                (category, argsType.FindRecentCompoundView.ObjectPointerViaContext, this, right);
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

        internal Result PrefixResult
            (Category category, Definable definable, SourcePart source, CompileSyntax right)
        {
            var searchResult = Declarations(definable);
            if(searchResult == null)
                return RootContext.UndefinedSymbol(source).Result(category);

            var result = searchResult.Execute
                (category, FindRecentCompoundView.ObjectPointerViaContext, this, right);
            Tracer.Assert(category <= result.CompleteCategory);
            return result;
        }

        internal virtual IEnumerable<ContextSearchResult> Declarations<TDefinable>
            (TDefinable tokenClass)
            where TDefinable : Definable
        {
            var provider = this as ISymbolProviderForPointer<TDefinable, IFeatureImplementation>;
            var feature = provider?.Feature(tokenClass);
            if(feature != null)
                yield return new
                    ContextSearchResult(feature, RootContext);
        }

        public IssueType UndefinedSymbol(SourcePart source)
            =>
                new RootIssueType
                    (
                    new Issue(IssueId.UndefinedSymbol, source, "Context: " + Dump()),
                    RootContext);
    }
}