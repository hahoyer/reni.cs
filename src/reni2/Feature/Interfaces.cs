using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.ReniSyntax;
using Reni.Struct;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Feature
{
    interface IFeatureImplementation
    {
        IMetaFunctionFeature Meta { get; }
        IFunctionFeature Function { get; }
        ISimpleFeature Simple { get; }
        IContextMetaFunctionFeature ContextMeta { get; }
    }

    sealed class EmptyFeatureImplementation : DumpableObject, IFeatureImplementation
    {
        internal EmptyFeatureImplementation(int? nextObjectId)
            : base(nextObjectId) { }
        internal EmptyFeatureImplementation() { }

        IContextMetaFunctionFeature IFeatureImplementation.ContextMeta => null;
        IMetaFunctionFeature IFeatureImplementation.Meta => null;
        IFunctionFeature IFeatureImplementation.Function => null;
        ISimpleFeature IFeatureImplementation.Simple => null;
    }

    abstract class SimpleFeatureImplementation : DumpableObject, IFeatureImplementation, ISimpleFeature
    {
        protected SimpleFeatureImplementation(int? nextObjectId)
            : base(nextObjectId) { }
        protected SimpleFeatureImplementation() { }

        IContextMetaFunctionFeature IFeatureImplementation.ContextMeta => null;
        IMetaFunctionFeature IFeatureImplementation.Meta => null;
        IFunctionFeature IFeatureImplementation.Function => null;
        ISimpleFeature IFeatureImplementation.Simple => this;
        Result ISimpleFeature.Result(Category category) => Result(category);
        TypeBase ISimpleFeature.TargetType => TargetType;

        protected abstract Result Result(Category category);
        protected abstract TypeBase TargetType { get; }
    }

    abstract class MetaFeatureImplementation : DumpableObject, IFeatureImplementation, IMetaFunctionFeature
    {
        protected MetaFeatureImplementation(int? nextObjectId)
            : base(nextObjectId)
        { }
        protected MetaFeatureImplementation() { }

        IContextMetaFunctionFeature IFeatureImplementation.ContextMeta => null;
        IMetaFunctionFeature IFeatureImplementation.Meta => this;
        IFunctionFeature IFeatureImplementation.Function => null;
        ISimpleFeature IFeatureImplementation.Simple => null;
        Result IMetaFunctionFeature.Result(Category category, ResultCache left, ContextBase contextBase, CompileSyntax right)
            => Result(category, left, contextBase, right);
        protected abstract Result Result(Category category, ResultCache left, ContextBase contextBase, CompileSyntax right);
    }

    abstract class FunctionFeatureImplementation : DumpableObject, IFeatureImplementation, IFunctionFeature
    {
        protected FunctionFeatureImplementation(int? nextObjectId)
            : base(nextObjectId)
        { }
        protected FunctionFeatureImplementation() { }

        IContextMetaFunctionFeature IFeatureImplementation.ContextMeta => null;
        IMetaFunctionFeature IFeatureImplementation.Meta => null;
        IFunctionFeature IFeatureImplementation.Function => this;
        ISimpleFeature IFeatureImplementation.Simple => null;
        Result IFunctionFeature.ApplyResult(Category category, TypeBase argsType)
            => ApplyResult(category, argsType);
        bool IFunctionFeature.IsImplicit => IsImplicit;
        IContextReference IFunctionFeature.ObjectReference => ObjectReference;

        protected abstract Result ApplyResult(Category category, TypeBase argsType);
        protected abstract bool IsImplicit { get; }
        protected abstract IContextReference ObjectReference { get; }
    }

    abstract class ContextMetaFeatureImplementation : DumpableObject, IFeatureImplementation, IContextMetaFunctionFeature
    {
        protected ContextMetaFeatureImplementation(int? nextObjectId)
            : base(nextObjectId)
        { }
        protected ContextMetaFeatureImplementation() { }

        IContextMetaFunctionFeature IFeatureImplementation.ContextMeta => this;
        IMetaFunctionFeature IFeatureImplementation.Meta => null;
        IFunctionFeature IFeatureImplementation.Function => null;
        ISimpleFeature IFeatureImplementation.Simple => null;
        Result IContextMetaFunctionFeature.Result(ContextBase contextBase, Category category, CompileSyntax right)
            => Result(contextBase, category, right);
        protected abstract Result Result(ContextBase contextBase, Category category, CompileSyntax right);
    }

    interface ISimpleFeature
    {
        Result Result(Category category);
        TypeBase TargetType { get; }
    }

    interface IFunctionFeature
    {
        /// <summary>
        ///     Result code contains CodeBase.Arg for argsType and ObjectReference for function object, if appropriate
        /// </summary>
        /// <param name="category"> </param>
        /// <param name="argsType"> </param>
        /// <returns> </returns>
        Result ApplyResult(Category category, TypeBase argsType);

        /// <summary>
        ///     Gets a value indicating whether this function requires implicit call (i. e. call without argument list).
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is implicit; otherwise, <c>false</c>.
        /// </value>
        [DisableDump]
        bool IsImplicit { get; }

        [DisableDump]
        IContextReference ObjectReference { get; }
    }

    interface IMetaFunctionFeature
    {
        Result Result(Category category, ResultCache left, ContextBase contextBase, CompileSyntax right);
    }

    interface IContextMetaFunctionFeature
    {
        Result Result(ContextBase contextBase, Category category, CompileSyntax right);
    }

    interface ISearchTarget {}

    // ReSharper disable once TypeParameterCanBeVariant
    // Exact match for TDefinable is required here.
    interface ISymbolProviderForPointer<TDefinable, out TPath>
        where TDefinable : Definable
    {
        TPath Feature(TDefinable tokenClass);
    }

    interface ISymbolProvider<TDefinable, out TPath>
        where TDefinable : Definable
    {
        TPath Feature(TDefinable tokenClass);
    }

    interface IGenericProviderForType
    {
        IEnumerable<ISimpleFeature> GetForcedConversions(TypeBase typeBase);
    }

    interface IGenericProviderForDefinable
    {
        IEnumerable<SearchResult> Declarations(TypeBase source);
        IEnumerable<ContextSearchResult> Declarations(ContextBase source);
    }

    sealed class GenericProviderForType<T> : DumpableObject, IGenericProviderForType
    {
        readonly T _target;
        public GenericProviderForType(T target) { _target = target; }

        IEnumerable<ISimpleFeature> IGenericProviderForType.GetForcedConversions(TypeBase source)
            => source.GetForcedConversions(_target);
    }

    sealed class GenericProviderForDefinable<T> : DumpableObject, IGenericProviderForDefinable
        where T : Definable
    {
        readonly T _target;
        public GenericProviderForDefinable(T target) { _target = target; }

        IEnumerable<SearchResult> IGenericProviderForDefinable.Declarations(TypeBase source) => source.Declarations(_target);
        IEnumerable<ContextSearchResult> IGenericProviderForDefinable.Declarations(ContextBase source)
            => source.Declarations(_target);
    }

    sealed class MetaFunction : DumpableObject, IFeatureImplementation, IMetaFunctionFeature
    {
        readonly Func<Category, ResultCache, ContextBase, CompileSyntax, Result> _function;
        public MetaFunction(Func<Category, ResultCache, ContextBase, CompileSyntax, Result> function) { _function = function; }

        IContextMetaFunctionFeature IFeatureImplementation.ContextMeta => null;
        IMetaFunctionFeature IFeatureImplementation.Meta => this;
        IFunctionFeature IFeatureImplementation.Function => null;
        ISimpleFeature IFeatureImplementation.Simple => null;

        Result IMetaFunctionFeature.Result(Category category, ResultCache left, ContextBase contextBase, CompileSyntax right)
            => _function(category, left, contextBase, right);
    }

    sealed class ContextMetaFunction : ContextMetaFeatureImplementation
    {
        readonly Func<ContextBase, Category, CompileSyntax, Result> _function;
        public ContextMetaFunction(Func<ContextBase, Category, CompileSyntax, Result> function) { _function = function; }

        protected override Result Result(ContextBase contextBase, Category category, CompileSyntax right)
            => _function(contextBase, category, right);
    }

    sealed class ContextMetaFunctionFromSyntax : DumpableObject, IFeatureImplementation, IContextMetaFunctionFeature
    {
        [EnableDump]
        readonly CompileSyntax _definition;
        public ContextMetaFunctionFromSyntax(CompileSyntax definition) { _definition = definition; }

        IContextMetaFunctionFeature IFeatureImplementation.ContextMeta => this;
        IMetaFunctionFeature IFeatureImplementation.Meta => null;
        IFunctionFeature IFeatureImplementation.Function => null;
        ISimpleFeature IFeatureImplementation.Simple => null;

        Result IContextMetaFunctionFeature.Result(ContextBase callContext, Category category, CompileSyntax right)
            => callContext.Result(category, _definition.ReplaceArg(right));
    }

    interface IStepRelative {}
}