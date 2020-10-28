using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using JetBrains.Annotations;
using Reni.Basics;
using Reni.Feature;
using Reni.Parser;
using Reni.Struct;
using Reni.SyntaxTree;
using Reni.TokenClasses;
using Reni.Type;
using Reni.Validation;

namespace Reni.Context
{
    /// <summary>
    ///     Base class for compiler environments
    /// </summary>
    abstract class ContextBase
        : DumpableObject,
            ResultCache.IResultProvider,
            IIconKeyProvider,
            ValueCache.IContainer,
            IRootProvider
    {
        internal sealed class ResultProvider : DumpableObject, ResultCache.IResultProvider
        {
            static int NextObjectId;

            [EnableDumpExcept(false)]
            readonly bool AsReference;

            internal readonly ContextBase Context;
            internal readonly ValueSyntax Syntax;

            internal ResultProvider
                (ContextBase context, ValueSyntax syntax, bool asReference = false)
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
        }

        internal sealed class Cache : DumpableObject, IIconKeyProvider
        {
            [Node]
            [SmartNode]
            internal readonly ResultCache AsObject;

            [Node]
            [SmartNode]
            internal readonly FunctionCache<CompoundSyntax, Compound> Compounds;

            [Node]
            [DisableDump]
            internal readonly ValueCache<IFunctionContext> RecentFunctionContextObject;

            [Node]
            [DisableDump]
            internal readonly ValueCache<CompoundView> RecentStructure;

            [Node]
            [SmartNode]
            internal readonly FunctionCache<ValueSyntax, ResultCache> ResultAsReferenceCache;

            [Node]
            [SmartNode]
            internal readonly FunctionCache<ValueSyntax, ResultCache> ResultCache;

            public Cache(ContextBase target)
            {
                ResultCache = new FunctionCache<ValueSyntax, ResultCache>
                    (target.ResultCacheForCache);
                ResultAsReferenceCache = new FunctionCache<ValueSyntax, ResultCache>
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

        static int NextId;

        [DisableDump]
        [Node]
        internal readonly Cache CacheObject;

        protected ContextBase()
            : base(NextId++) => CacheObject = new Cache(this);

        ValueCache ValueCache.IContainer.Cache { get; } = new ValueCache();

        string IIconKeyProvider.IconKey => "Context";


        Result ResultCache.IResultProvider.Execute(Category category, Category pendingCategory)
        {
            if(pendingCategory == Category.None)
                return FindRecentCompoundView.ObjectPointerViaContext(category);

            NotImplementedMethod(category, pendingCategory);
            return null;
        }

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

        internal virtual IEnumerable<ContextBase> ParentChain { get { yield return this; } }

        [DisableDump]
        public string Format => ParentChain.Select(item => item.LevelFormat).Stringify(separator: " in ");

        [DisableDump]
        protected abstract string LevelFormat { get; }

        protected override string GetNodeDump()
            => base.GetNodeDump() + "(" + GetContextIdentificationDump() + ")";

        public abstract string GetContextIdentificationDump();

        [UsedImplicitly]
        internal int SizeToPacketCount(Size size)
            => size.SizeToPacketCount(Root.DefaultRefAlignParam.AlignBits);

        internal ContextBase CompoundPositionContext(CompoundSyntax container, int? position = null)
            => CompoundView(container, position)?.CompoundContext;

        internal CompoundView CompoundView(CompoundSyntax syntax, int? accessPosition = null)
            => CacheObject.Compounds[syntax].View[accessPosition ?? syntax.EndPosition];

        internal Compound Compound(CompoundSyntax context) => CacheObject.Compounds[context];

        //[DebuggerHidden]
        internal Result Result(Category category, ValueSyntax syntax)
            => ResultCache(syntax).GetCategories(category);

        internal ResultCache ResultCache(ValueSyntax syntax)
            => CacheObject.ResultCache[syntax];

        internal ResultCache ResultAsReferenceCache(ValueSyntax syntax)
            => CacheObject.ResultAsReferenceCache[syntax];

        internal TypeBase TypeIfKnown(ValueSyntax syntax)
            => CacheObject.ResultCache[syntax].Data.Type;

        //[DebuggerHidden]
        Result ResultForCache(Category category, ValueSyntax syntax)
        {
            var trace = syntax.ObjectId.In() && ObjectId.In(7) && category.HasType;
            StartMethodDump(trace, category, syntax);
            try
            {
                BreakExecution();
                var result = syntax.ResultForCache(this, category.Replenished);
                (result == null || result.IsValidOrIssue(category)).Assert();
                return ReturnMethodDump(result);
            }
            finally
            {
                EndMethodDump();
            }
        }

        [DebuggerHidden]
        ResultCache ResultCacheForCache(ValueSyntax syntax)
        {
            var result = new ResultCache(new ResultProvider(this, syntax));
            syntax.AddToCacheForDebug(this, result);
            return result;
        }

        ResultCache GetResultAsReferenceCacheForCache(ValueSyntax syntax)
            => new ResultCache(new ResultProvider(this, syntax, asReference: true));

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

        internal Result ResultAsReference(Category category, ValueSyntax syntax)
            => Result(category.WithType, syntax)
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
        internal Result FunctionalArgResult(Category category, ValueSyntax right, SourcePart token)
        {
            var argsType = FindRecentFunctionContextObject.ArgsType;
            return argsType
                .Execute
                (
                    category,
                    new ResultCache(FunctionalArgObjectResult),
                    token,
                    definable: null,
                    context: this,
                    right: right
                );
        }

        Result FunctionalArgObjectResult(Category category)
        {
            NotImplementedMethod(category);
            return null;
        }

        IImplementation Declaration(Definable tokenClass)
        {
            var genericTokenClass = tokenClass.MakeGeneric.ToArray();
            var results 
                = genericTokenClass
                    .SelectMany(g => g.Declarations(this));
            var result = results.SingleOrDefault();
            if(result != null || RootContext.ProcessErrors)
                return result;

            NotImplementedMethod(tokenClass);
            return null;
        }

        internal Result PrefixResult
            (Category category, Definable definable, SourcePart source, ValueSyntax right)
        {
            var searchResult = Declaration(definable);
            if(searchResult == null)
                return IssueId
                    .MissingDeclarationInContext
                    .IssueResult(category, source, "Context: " + RootContext.Format);

            var result = searchResult.Result(category, CacheObject.AsObject, source, this, right);

            Tracer.Assert(result.HasIssue || category <= result.CompleteCategory);
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

        public Result CreateArrayResult(Category category, ValueSyntax argsType, bool isMutable)
        {
            var target = Result(category.WithType, argsType).SmartUn<PointerType>().Align;
            return target
                       .Type
                       .Array(1, ArrayType.Options.Create().IsMutable.SetTo(isMutable))
                       .Result(category.WithType, target)
                       .LocalReferenceResult
                   & category;
        }
    }

    sealed class SmartNodeAttribute : Attribute
    {
    }
}