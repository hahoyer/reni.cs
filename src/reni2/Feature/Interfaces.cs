using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Basics;
using Reni.Context;
using Reni.Parser;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Feature
{
    interface ITypedFeatureImplementation
    {
        IFunctionFeature Function { get; }
        IValueFeature Value { get; }
    }

    interface IMetaFeatureImplementation
    {
        IMetaFunctionFeature Meta { get; }
    }

    interface IContextMetaFeatureImplementation
    {
        IContextMetaFunctionFeature ContextMeta { get; }
    }

    interface IFeatureImplementation
        : ITypedFeatureImplementation
            , IMetaFeatureImplementation {}

    interface IContextFeatureImplementation
        : ITypedFeatureImplementation
            , IContextMetaFeatureImplementation {}

    interface ICommonFeatureImplementation
        : IContextFeatureImplementation
            , IFeatureImplementation {}

    abstract class FunctionFeatureImplementation
        : DumpableObject, IFeatureImplementation, IFunctionFeature
    {
        protected FunctionFeatureImplementation(int? nextObjectId)
            : base(nextObjectId) {}

        protected FunctionFeatureImplementation() { }

        IMetaFunctionFeature IMetaFeatureImplementation.Meta => null;
        IFunctionFeature ITypedFeatureImplementation.Function => this;
        IValueFeature ITypedFeatureImplementation.Value => null;

        Result IFunctionFeature.Result(Category category, TypeBase argsType)
            => Result(category, argsType);

        bool IFunctionFeature.IsImplicit => IsImplicit;

        protected abstract Result Result(Category category, TypeBase argsType);
        protected abstract bool IsImplicit { get; }
    }

    abstract class ContextMetaFeatureImplementation
        : DumpableObject, IContextFeatureImplementation, IContextMetaFunctionFeature
    {
        IContextMetaFunctionFeature IContextMetaFeatureImplementation.ContextMeta => this;
        IFunctionFeature ITypedFeatureImplementation.Function => null;
        IValueFeature ITypedFeatureImplementation.Value => null;

        Result IContextMetaFunctionFeature.Result
            (ContextBase contextBase, Category category, CompileSyntax right)
            => Result(contextBase, category, right);

        protected abstract Result Result
            (ContextBase contextBase, Category category, CompileSyntax right);
    }

    interface IValueFeature
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
        Result Result(Category category, TypeBase argsType);

        /// <summary>
        ///     Gets a value indicating whether this function requires implicit call (i. e. call without argument list).
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is implicit; otherwise, <c>false</c>.
        /// </value>
        [DisableDump]
        bool IsImplicit { get; }
    }

    interface IMetaFunctionFeature
    {
        Result Result
            (Category category, ResultCache left, ContextBase contextBase, CompileSyntax right);
    }

    interface IContextMetaFunctionFeature
    {
        Result Result(ContextBase contextBase, Category category, CompileSyntax right);
    }

    interface ISearchTarget {}

    // ReSharper disable once TypeParameterCanBeVariant
    // Exact match for TDefinable is required here.
    interface ISymbolProviderForContext<TDefinable>
        where TDefinable : Definable
    {
        IContextFeatureImplementation Feature(TDefinable tokenClass);
    }

    // ReSharper disable once TypeParameterCanBeVariant
    // Exact match for TDefinable is required here.
    interface ISymbolProviderForPointer<TDefinable>
        where TDefinable : Definable
    {
        IFeatureImplementation Feature(TDefinable tokenClass);
    }

    // ReSharper disable once TypeParameterCanBeVariant
    // Exact match for TDefinable is required here.
    interface ISymbolProvider<TDefinable>
        where TDefinable : Definable
    {
        IFeatureImplementation Feature(TDefinable tokenClass);
    }

    interface IGenericProviderForType
    {
        IEnumerable<IValueFeature> GetForcedConversions(TypeBase typeBase);
    }

    interface IDeclarationProvider
    {
        IEnumerable<SearchResult> Declarations(TypeBase source);
        IEnumerable<IContextFeatureImplementation> Declarations(ContextBase source);
    }

    sealed class GenericProviderForType<T> : DumpableObject, IGenericProviderForType
    {
        readonly T _target;
        public GenericProviderForType(T target) { _target = target; }

        IEnumerable<IValueFeature> IGenericProviderForType.GetForcedConversions(TypeBase source)
            => source.GetForcedConversions(_target);
    }

    sealed class GenericProviderForDefinable<T> : DumpableObject, IDeclarationProvider
        where T : Definable
    {
        readonly T _target;
        public GenericProviderForDefinable(T target) { _target = target; }

        IEnumerable<SearchResult> IDeclarationProvider.Declarations(TypeBase source)
            => source.Declarations(_target);

        IEnumerable<IContextFeatureImplementation> IDeclarationProvider.Declarations
            (ContextBase source)
            => source.Declarations(_target);
    }

    sealed class MetaFunction : DumpableObject, IFeatureImplementation, IMetaFunctionFeature
    {
        readonly Func<Category, ResultCache, ContextBase, CompileSyntax, Result> _function;

        public MetaFunction
            (Func<Category, ResultCache, ContextBase, CompileSyntax, Result> function)
        {
            _function = function;
        }

        IMetaFunctionFeature IMetaFeatureImplementation.Meta => this;
        IFunctionFeature ITypedFeatureImplementation.Function => null;
        IValueFeature ITypedFeatureImplementation.Value => null;

        Result IMetaFunctionFeature.Result
            (Category category, ResultCache left, ContextBase contextBase, CompileSyntax right)
            => _function(category, left, contextBase, right);
    }

    sealed class ContextMetaFunction : ContextMetaFeatureImplementation
    {
        readonly Func<ContextBase, Category, CompileSyntax, Result> _function;

        public ContextMetaFunction(Func<ContextBase, Category, CompileSyntax, Result> function)
        {
            _function = function;
        }

        protected override Result Result
            (ContextBase contextBase, Category category, CompileSyntax right)
            => _function(contextBase, category, right);
    }

    sealed class ContextMetaFunctionFromSyntax
        : DumpableObject, IContextFeatureImplementation, IContextMetaFunctionFeature
    {
        [EnableDump]
        readonly CompileSyntax _definition;
        public ContextMetaFunctionFromSyntax(CompileSyntax definition) { _definition = definition; }

        IContextMetaFunctionFeature IContextMetaFeatureImplementation.ContextMeta => this;
        IFunctionFeature ITypedFeatureImplementation.Function => null;
        IValueFeature ITypedFeatureImplementation.Value => null;

        Result IContextMetaFunctionFeature.Result
            (ContextBase callContext, Category category, CompileSyntax right)
            => callContext.Result(category, _definition.ReplaceArg(right));
    }

    interface IStepRelative {}
}