using System;
using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.SyntaxTree;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Feature
{
    interface IEvalImplementation
    {
        IFunction Function { get; }
        IValue Value { get; }
    }

    interface IMetaImplementation
    {
        IMeta Function { get; }
    }

    interface IImplementation : IEvalImplementation, IMetaImplementation {}

    abstract class FunctionFeatureImplementation
        : DumpableObject, IImplementation, IFunction
    {
        protected FunctionFeatureImplementation(int? nextObjectId)
            : base(nextObjectId) {}

        protected FunctionFeatureImplementation() { }

        IMeta IMetaImplementation.Function => null;
        IFunction IEvalImplementation.Function => this;
        IValue IEvalImplementation.Value => null;

        Result IFunction.Result(Category category, TypeBase argsType)
            => Result(category, argsType);

        bool IFunction.IsImplicit => IsImplicit;

        protected abstract Result Result(Category category, TypeBase argsType);
        protected abstract bool IsImplicit { get; }
    }

    abstract class ContextMetaFeatureImplementation
        : DumpableObject
            , IImplementation
            , IMeta
    {
        IMeta IMetaImplementation.Function => this;
        IFunction IEvalImplementation.Function => null;
        IValue IEvalImplementation.Value => null;

        Result IMeta.Result
            (Category category, ResultCache left, ContextBase contextBase, ValueSyntax right)
            => Result(contextBase, category, right);

        protected abstract Result Result
            (ContextBase contextBase, Category category, ValueSyntax right);
    }

    /// <summary>
    ///     Provide the result as a transformation from arg of type Source
    /// </summary>
    interface IConversion
    {
        Result Execute(Category category);
        TypeBase Source { get; }
    }

    interface IValue
    {
        Result Execute(Category category);
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
            (Category category, ResultCache left, ContextBase contextBase, ValueSyntax right);
    }

    interface ISearchTarget {}

    // ReSharper disable once TypeParameterCanBeVariant
    // Exact match for TDefinable is required here.
    interface ISymbolProviderForPointer<TDefinable>
        where TDefinable : Definable
    {
        IImplementation Feature(TDefinable tokenClass);
    }

    // ReSharper disable once TypeParameterCanBeVariant
    // Exact match for TDefinable is required here.
    interface ISymbolProvider<TDefinable>
        where TDefinable : Definable
    {
        IImplementation Feature(TDefinable tokenClass);
    }

    interface IGenericProviderForType
    {
        IEnumerable<IConversion> GetForcedConversions(TypeBase typeBase);
    }

    interface IDeclarationProvider
    {
        IEnumerable<SearchResult> Declarations(TypeBase source);
        IEnumerable<IImplementation> Declarations(ContextBase source);
    }

    sealed class GenericProviderForType<T> : DumpableObject, IGenericProviderForType
    {
        readonly T _target;
        public GenericProviderForType(T target) { _target = target; }

        IEnumerable<IConversion> IGenericProviderForType.GetForcedConversions(TypeBase source)
            => source.GetForcedConversions(_target);
    }

    sealed class GenericProviderForDefinable<T> : DumpableObject, IDeclarationProvider
        where T : Definable
    {
        readonly T _target;
        public GenericProviderForDefinable(T target) { _target = target; }

        IEnumerable<SearchResult> IDeclarationProvider.Declarations(TypeBase source)
            => source.Declarations(_target);

        IEnumerable<IImplementation> IDeclarationProvider.Declarations(ContextBase source)
            => source.Declarations(_target);
    }

    sealed class MetaFunction : DumpableObject, IImplementation, IMeta
    {
        readonly Func<Category, ResultCache, ContextBase, ValueSyntax, Result> _function;

        public MetaFunction
            (Func<Category, ResultCache, ContextBase, ValueSyntax, Result> function)
        {
            _function = function;
        }

        IMeta IMetaImplementation.Function => this;
        IFunction IEvalImplementation.Function => null;
        IValue IEvalImplementation.Value => null;

        Result IMeta.Result
            (Category category, ResultCache left, ContextBase contextBase, ValueSyntax right)
            => _function(category, left, contextBase, right);
    }

    sealed class ContextMetaFunction : ContextMetaFeatureImplementation
    {
        readonly Func<ContextBase, Category, ValueSyntax, Result> _function;

        public ContextMetaFunction(Func<ContextBase, Category, ValueSyntax, Result> function)
        {
            _function = function;
        }

        protected override Result Result
            (ContextBase contextBase, Category category, ValueSyntax right)
            => _function(contextBase, category, right);
    }

    sealed class ContextMetaFunctionFromSyntax
        : DumpableObject, IImplementation, IMeta
    {
        [EnableDump]
        readonly ValueSyntax _definition;
        public ContextMetaFunctionFromSyntax(ValueSyntax definition) { _definition = definition; }

        IMeta IMetaImplementation.Function => this;
        IFunction IEvalImplementation.Function => null;
        IValue IEvalImplementation.Value => null;

        Result IMeta.Result
            (Category category, ResultCache left, ContextBase callContext, ValueSyntax right)
            => callContext.Result(category, _definition.ReplaceArg(right));
    }

    interface IForcedConversionProvider<in TDestination>
    {
        IEnumerable<IConversion> Result(TDestination destination);
    }

    interface IForcedConversionProviderForPointer<in TDestination>
    {
        IEnumerable<IConversion> Result(TDestination destination);
    }

    interface IChild<out TParent>
    {
        TParent Parent { get; }
    }

    interface ISourceProvider
    {
        SourcePart Value { get; }
    }
}