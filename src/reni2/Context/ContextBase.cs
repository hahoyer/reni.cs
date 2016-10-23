using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using JetBrains.Annotations;
using Reni.Basics;
using Reni.Feature;
using Reni.Struct;
using Reni.TokenClasses;
using Reni.Type;
using Reni.Validation;

namespace Reni.Context
{
    /// <summary>
    ///     Base class for compiler environments
    /// </summary>
    abstract class ContextBase : DumpableObject,
        ResultCache.IResultProvider,
        IIconKeyProvider,
        ValueCache.IContainer,
        IRootProvider
    {
        protected override string GetNodeDump()
            => base.GetNodeDump() + "(" + GetContextIdentificationDump() + ")";

        static int NextId;

        [DisableDump]
        [Node]
        internal readonly Cache CacheObject;

        protected ContextBase()
            : base(NextId++)
        {
            CacheObject = new Cache(this);
        }

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

        [DisableDump]
        internal virtual IEnumerable<string> DeclarationOptions
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }

        [UsedImplicitly]
        internal int SizeToPacketCount(Size size)
            => size.SizeToPacketCount(Root.DefaultRefAlignParam.AlignBits);

        internal ContextBase CompoundPositionContext(CompoundSyntax container, int? position = null)
            => CompoundView(container, position)?.CompoundContext;

        internal CompoundView CompoundView(CompoundSyntax syntax, int? accessPosition = null)
            => CacheObject.Compounds[syntax].View[accessPosition ?? syntax.EndPosition];

        internal Compound Compound(CompoundSyntax context) => CacheObject.Compounds[context];

        [DebuggerHidden]
        internal Result Result(Category category, Parser.Value syntax)
            => ResultCache(syntax).GetCategories(category);

        internal ResultCache ResultCache(Parser.Value syntax)
            => CacheObject.ResultCache[syntax];

        internal ResultCache ResultAsReferenceCache(Parser.Value syntax)
            => CacheObject.ResultAsReferenceCache[syntax];

        internal TypeBase TypeIfKnown(Parser.Value syntax)
            => CacheObject.ResultCache[syntax].Data.Type;

        [DebuggerHidden]
        Result ResultForCache(Category category, Parser.Value syntax)
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
            internal readonly Parser.Value Syntax;
            static int NextObjectId;

            internal ResultProvider
                (ContextBase context, Parser.Value syntax, bool asReference = false)
                : base(NextObjectId++)
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

            [EnableDump]
            string ContextId => Context.NodeDump;

            [EnableDump]
            int SyntaxObjectId => Syntax.ObjectId;

            [EnableDump]
            string SyntaxText => Syntax.SourcePart.Id;
        }

        [DebuggerHidden]
        ResultCache ResultCacheForCache(Parser.Value syntax)
        {
            var result = new ResultCache(new ResultProvider(this, syntax));
            syntax.AddToCacheForDebug(this, result);
            return result;
        }

        ResultCache GetResultAsReferenceCacheForCache(Parser.Value syntax)
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
            internal readonly FunctionCache<Parser.Value, ResultCache> ResultCache;

            [Node]
            [SmartNode]
            internal readonly FunctionCache<Parser.Value, ResultCache> ResultAsReferenceCache;

            [Node]
            [SmartNode]
            internal readonly ResultCache AsObject;

            public Cache(ContextBase target)
            {
                ResultCache = new FunctionCache<Parser.Value, ResultCache>
                    (target.ResultCacheForCache);
                ResultAsReferenceCache = new FunctionCache<Parser.Value, ResultCache>
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

        internal Result ResultAsReference(Category category, Parser.Value syntax)
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
            (Category category, Parser.Value right, Syntax token)
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
            (Category category, Definable definable, Syntax source, Parser.Value right)
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

        IssueType UndefinedSymbol(Syntax source)
            =>
                new RootIssueType
                    (
                    new Issue(IssueId.MissingDeclarationInContext, source, "Context: " + Format),
                    RootContext);


        Result ResultCache.IResultProvider.Execute(Category category, Category pendingCategory)
        {
            if(pendingCategory == Category.None)
                return FindRecentCompoundView.ObjectPointerViaContext(category);

            NotImplementedMethod(category, pendingCategory);
            return null;
        }

        internal virtual IEnumerable<ContextBase> ParentChain { get { yield return this; } }

        ValueCache ValueCache.IContainer.Cache { get; } = new ValueCache();

        [DisableDump]
        public string Format => ParentChain.Select(item=>item.LevelFormat).Stringify(" in ");
        [DisableDump]
        protected abstract string LevelFormat { get; }
    }

    sealed class SmartNodeAttribute : Attribute {}
}