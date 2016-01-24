using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using hw.DebugFormatter;
using hw.Forms;
using hw.Helper;
using hw.Scanner;
using JetBrains.Annotations;
using Reni.Basics;
using Reni.Code;
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
            , ResultCache.IResultProvider
            , IIconKeyProvider
            , ValueCache.IContainer          
        ,IRootProvider
    {
        protected override string GetNodeDump()
            => base.GetNodeDump() + "(" + GetContextIdentificationDump() + ")";

        static int _nextId;

        [DisableDump]
        [Node]
        internal readonly Cache CacheObject;

        protected ContextBase()
            : base(_nextId++) { CacheObject = new Cache(this); }

        public abstract string GetContextIdentificationDump();

        string IIconKeyProvider.IconKey => "Context";

        Root IRootProvider.Value => RootContext;

        [DisableDump]
        [Node]
        internal abstract Root RootContext { get; }

        [DisableDump]
        internal CompoundView FindRecentCompoundView => CacheObject.RecentStructure.Value;

        [DisableDump]
        internal IFunctionContext FindRecentFunctionContextObject
            => CacheObject.RecentFunctionContextObject.Value;

        [DisableDump]
        internal abstract bool IsRecursionMode { get; }

        [UsedImplicitly]
        internal int SizeToPacketCount(Size size)
            => size.SizeToPacketCount(Root.DefaultRefAlignParam.AlignBits);

        internal ContextBase CompoundPositionContext(CompoundSyntax container, int position)
            => CompoundView(container, position).CompoundContext;

        internal CompoundView CompoundView(CompoundSyntax syntax, int? accessPosition = null)
            => CacheObject.Compounds[syntax].View[accessPosition ?? syntax.EndPosition];

        internal Compound Compound(CompoundSyntax context) => CacheObject.Compounds[context];

        [DebuggerHidden]
        internal Result Result(Category category, CompileSyntax syntax)
            => ResultCache(syntax).GetCategories(category);

        internal ResultCache ResultCache(CompileSyntax syntax) => CacheObject.ResultCache[syntax];

        internal ResultCache ResultAsReferenceCache(CompileSyntax syntax)
            => CacheObject.ResultAsReferenceCache[syntax];


        internal TypeBase TypeIfKnown(CompileSyntax syntax)
            => CacheObject.ResultCache[syntax].Data.Type;

        [DebuggerHidden]
        Result ResultForCache(Category category, CompileSyntax syntax)
        {
            var trace = syntax.ObjectId == 10 && ObjectId == 1769 && category.HasType;
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

        internal sealed class ResultProvider : DumpableObject, ResultCache.IResultProvider
        {
            [EnableDumpExcept(false)]
            readonly bool AsReference;
            internal readonly ContextBase Context;
            internal readonly CompileSyntax Syntax;
            static int _nextObjectId;

            internal ResultProvider
                (ContextBase context, CompileSyntax syntax, bool asReference = false)
                : base(_nextObjectId++)
            {
                Context = context;
                Syntax = syntax;
                AsReference = asReference;
                StopByObjectIds();
            }

            Result ResultCache.IResultProvider.Execute(Category category, Category pendingCategory)
            {
                if(pendingCategory.IsNone)
                    return AsReference
                        ? Context.ResultAsReference(category, Syntax)
                        : Context.ResultForCache(category, Syntax);

                var recursionHandler = Syntax.RecursionHandler;
                if(recursionHandler != null)
                    return recursionHandler
                        .Execute(Context, category, pendingCategory, Syntax, AsReference);

                NotImplementedMethod(category, pendingCategory);
                return null;
            }

            ResultCache.IResultProvider ResultCache.IResultProvider.FindSource
                (IContextReference ext)
                => Context.FindSource(Syntax, ext);

            [EnableDump]
            string ContextId => Context.NodeDump;

            [EnableDump]
            int SyntaxObjectId => Syntax.ObjectId;

            [EnableDump]
            string SyntaxText => Syntax.SourcePart.Id;
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

        internal sealed class Cache : DumpableObject, IIconKeyProvider
        {
            [Node]
            [DisableDump]
            internal readonly ValueCache<CompoundView> RecentStructure;

            [Node]
            [DisableDump]
            internal readonly ValueCache<IFunctionContext> RecentFunctionContextObject;

            [Node]
            [SmartNode]
            internal readonly FunctionCache<CompoundSyntax, Compound> Compounds;

            [Node]
            [SmartNode]
            internal readonly FunctionCache<CompileSyntax, ResultCache> ResultCache;

            [Node]
            [SmartNode]
            internal readonly FunctionCache<CompileSyntax, ResultCache> ResultAsReferenceCache;

            [Node]
            [SmartNode]
            internal readonly ResultCache AsObject;

            public Cache(ContextBase target)
            {
                ResultCache = new FunctionCache<CompileSyntax, ResultCache>
                    (target.ResultCacheForCache);
                ResultAsReferenceCache = new FunctionCache<CompileSyntax, ResultCache>
                    (target.GetResultAsReferenceCacheForCache);
                RecentStructure = new ValueCache<CompoundView>(target.ObtainRecentCompoundView);
                RecentFunctionContextObject = new ValueCache<IFunctionContext>
                    (target.ObtainRecentFunctionContext);
                Compounds = new FunctionCache<CompoundSyntax, Compound>
                    (container => new Compound(container, target));

                AsObject = new ResultCache(target);
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
        /// <param name="token"></param>
        /// <returns> </returns>
        internal Result FunctionalArgResult
            (Category category, CompileSyntax right, SourcePart token)
        {
            var argsType = FindRecentFunctionContextObject.ArgsType;
            return argsType
                .Execute
                (
                    category,
                    new ResultCache(FunctionalArgObjectResult),
                    token,
                    null,
                    this,
                    right
                );
        }

        Result FunctionalArgObjectResult(Category category)
        {
            NotImplementedMethod(category);
            return null;
        }

        IImplementation Declaration(Definable tokenClass)
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
            var searchResult = Declaration(definable);
            if(searchResult == null)
                return RootContext.UndefinedSymbol(source).Result(category);

            var result = searchResult.Result(category, CacheObject.AsObject, source, this, right);

            Tracer.Assert(category <= result.CompleteCategory);
            return result;
        }

        internal virtual IEnumerable<IImplementation> Declarations<TDefinable>
            (TDefinable tokenClass)
            where TDefinable : Definable
        {
            var provider = this as ISymbolProviderForPointer<TDefinable>;
            var feature = provider?.Feature(tokenClass);
            if(feature != null)
                yield return feature;
        }

        IssueType UndefinedSymbol(SourcePart source)
            =>
                new RootIssueType
                    (
                    new Issue(IssueId.MissingDeclaration, source, "Context: " + Dump()),
                    RootContext);

        Result ResultCache.IResultProvider.Execute(Category category, Category pendingCategory)
        {
            if(pendingCategory == Category.None)
                return FindRecentCompoundView.ObjectPointerViaContext(category);
            NotImplementedMethod(category, pendingCategory);
            return null;
        }

        ResultCache.IResultProvider ResultCache.IResultProvider.FindSource(IContextReference ext)
        {
            NotImplementedMethod(ext);
            return null;
        }

        virtual internal IEnumerable<ContextBase> ParentChain { get { yield return this; } }

        ResultCache.IResultProvider FindSource(CompileSyntax syntax, IContextReference ext)
        {
            Tracer.Assert(syntax.ResultCache[this].Exts.Contains(ext));
            return syntax.FindSource(ext, this);
        }

        internal IEnumerable<ResultCache.IResultProvider> FindSourceChain
            (CompileSyntax syntax, IContextReference ext)
        {
            var current = FindSource(syntax, ext);
            while(current != null)
            {
                yield return current;
                current = current.FindSource(ext);
            }
        }

        internal IEnumerable<ResultCache.IResultProvider> GetDefinableResults
            (IContextReference ext, Definable definable, CompileSyntax right)
            => Declaration(definable).GetDefinableResults(ext, this, right);

        ValueCache ValueCache.IContainer.Cache { get; } = new ValueCache();

    }
}