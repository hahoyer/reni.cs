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
    interface IImplementation
    {
        IFunction Function { get; }
        IValue Value { get; }
    }

    interface IMetaImplementation
    {
        IMeta Function { get; }
    }

    interface IContextMetaImplementation
    {
        IContextMeta Function { get; }
    }

    interface ITypeImplementation
        : IImplementation
            , IMetaImplementation {}

    interface IContextImplementation
        : IImplementation
            , IContextMetaImplementation {}

    interface ICommonImplementation
        : IContextImplementation
            , ITypeImplementation {}

    abstract class FunctionFeatureImplementation
        : DumpableObject, ITypeImplementation, IFunction
    {
        protected FunctionFeatureImplementation(int? nextObjectId)
            : base(nextObjectId) {}

        protected FunctionFeatureImplementation() { }

        IMeta IMetaImplementation.Function => null;
        IFunction IImplementation.Function => this;
        IValue IImplementation.Value => null;

        Result IFunction.Result(Category category, TypeBase argsType)
            => Result(category, argsType);

        bool IFunction.IsImplicit => IsImplicit;

        protected abstract Result Result(Category category, TypeBase argsType);
        protected abstract bool IsImplicit { get; }
    }

    abstract class ContextMetaFeatureImplementation
        : DumpableObject, IContextImplementation, IContextMeta
    {
        IContextMeta IContextMetaImplementation.Function => this;
        IFunction IImplementation.Function => null;
        IValue IImplementation.Value => null;

        Result IContextMeta.Result
            (ContextBase contextBase, Category category, CompileSyntax right)
            => Result(contextBase, category, right);

        protected abstract Result Result
            (ContextBase contextBase, Category category, CompileSyntax right);
    }

    interface IValue
    {
        Result Result(Category category);
        TypeBase TargetType { get; }
    }

    interface IFunction
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

    interface IMeta
    {
        Result Result
            (Category category, ResultCache left, ContextBase contextBase, CompileSyntax right);
    }

    interface IContextMeta
    {
        Result Result(ContextBase contextBase, Category category, CompileSyntax right);
    }

    interface ISearchTarget {}

    // ReSharper disable once TypeParameterCanBeVariant
    // Exact match for TDefinable is required here.
    interface ISymbolProviderForContext<TDefinable>
        where TDefinable : Definable
    {
        IContextImplementation Feature(TDefinable tokenClass);
    }

    // ReSharper disable once TypeParameterCanBeVariant
    // Exact match for TDefinable is required here.
    interface ISymbolProviderForPointer<TDefinable>
        where TDefinable : Definable
    {
        ITypeImplementation Feature(TDefinable tokenClass);
    }

    // ReSharper disable once TypeParameterCanBeVariant
    // Exact match for TDefinable is required here.
    interface ISymbolProvider<TDefinable>
        where TDefinable : Definable
    {
        ITypeImplementation Feature(TDefinable tokenClass);
    }

    interface IGenericProviderForType
    {
        IEnumerable<IValue> GetForcedConversions(TypeBase typeBase);
    }

    interface IDeclarationProvider
    {
        IEnumerable<SearchResult> Declarations(TypeBase source);
        IEnumerable<IContextImplementation> Declarations(ContextBase source);
    }

    sealed class GenericProviderForType<T> : DumpableObject, IGenericProviderForType
    {
        readonly T _target;
        public GenericProviderForType(T target) { _target = target; }

        IEnumerable<IValue> IGenericProviderForType.GetForcedConversions(TypeBase source)
            => source.GetForcedConversions(_target);
    }

    sealed class GenericProviderForDefinable<T> : DumpableObject, IDeclarationProvider
        where T : Definable
    {
        readonly T _target;
        public GenericProviderForDefinable(T target) { _target = target; }

        IEnumerable<SearchResult> IDeclarationProvider.Declarations(TypeBase source)
            => source.Declarations(_target);

        IEnumerable<IContextImplementation> IDeclarationProvider.Declarations
            (ContextBase source)
            => source.Declarations(_target);
    }

    sealed class MetaFunction : DumpableObject, ITypeImplementation, IMeta
    {
        readonly Func<Category, ResultCache, ContextBase, CompileSyntax, Result> _function;

        public MetaFunction
            (Func<Category, ResultCache, ContextBase, CompileSyntax, Result> function)
        {
            _function = function;
        }

        IMeta IMetaImplementation.Function => this;
        IFunction IImplementation.Function => null;
        IValue IImplementation.Value => null;

        Result IMeta.Result
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
        : DumpableObject, IContextImplementation, IContextMeta
    {
        [EnableDump]
        readonly CompileSyntax _definition;
        public ContextMetaFunctionFromSyntax(CompileSyntax definition) { _definition = definition; }

        IContextMeta IContextMetaImplementation.Function => this;
        IFunction IImplementation.Function => null;
        IValue IImplementation.Value => null;

        Result IContextMeta.Result
            (ContextBase callContext, Category category, CompileSyntax right)
            => callContext.Result(category, _definition.ReplaceArg(right));
    }

    interface IStepRelative {}
}