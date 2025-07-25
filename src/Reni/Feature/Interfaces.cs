using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.SyntaxTree;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Feature;

interface IEvalImplementation
{
    IFunction? Function { get; }
    IValue? Value { get; }
}

interface IMetaImplementation
{
    IMeta? Function { get; }
}

interface IImplementation : IEvalImplementation, IMetaImplementation;

abstract class FunctionFeatureImplementation
    : DumpableObject, IImplementation, IFunction
{
    protected FunctionFeatureImplementation(int? nextObjectId)
        : base(nextObjectId) { }

    protected FunctionFeatureImplementation() { }
    IFunction IEvalImplementation.Function => this;
    IValue? IEvalImplementation.Value => null;

    Result IFunction.GetResult(Category category, TypeBase argsType)
        => GetResult(category, argsType);

    bool IFunction.IsImplicit => IsImplicit;

    IMeta? IMetaImplementation.Function => null;

    protected abstract Result GetResult(Category category, TypeBase argsType);
    protected abstract bool IsImplicit { get; }
}

abstract class ContextMetaFeatureImplementation
    : DumpableObject
        , IImplementation
        , IMeta
{
    IFunction? IEvalImplementation.Function => null;
    IValue? IEvalImplementation.Value => null;

    Result IMeta.GetResult
        (Category category, ResultCache left, SourcePart _, ContextBase contextBase, ValueSyntax? right)
        => GetResult(category, contextBase, right);

    IMeta IMetaImplementation.Function => this;

    protected abstract Result GetResult(Category category, ContextBase contextBase, ValueSyntax? right);
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
    ///     GetResult code contains CodeBase.Argument for argsType and ObjectReference for function object, if appropriate
    /// </summary>
    /// <param name="category"> </param>
    /// <param name="argsType"> </param>
    /// <returns> </returns>
    Result GetResult(Category category, TypeBase argsType);

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
    Result GetResult
    (
        Category category
        , ResultCache left
        , SourcePart token
        , ContextBase contextBase
        , ValueSyntax? right
    );
}

interface ISearchTarget;

// ReSharper disable once TypeParameterCanBeVariant
// Exact match for TDefinable is required here.
interface IMultiSymbolProviderForPointer<TDefinable>
    where TDefinable : Definable
{
    IImplementation? GetFeature(TDefinable tokenClass);
}

// ReSharper disable once UnusedTypeParameter
// Exact match for TDefinable is required here.
interface ISymbolProviderForPointer<TDefinable>
    where TDefinable : Definable
{
    IImplementation? Feature { get; }
}

// ReSharper disable once TypeParameterCanBeVariant
// Exact match for TDefinable is required here.
interface IMultiSymbolProvider<TDefinable>
    where TDefinable : Definable
{
    IImplementation? GetFeature(TDefinable tokenClass);
}

// ReSharper disable once UnusedTypeParameter
// Exact match for TDefinable is required here.
interface ISymbolProvider<TDefinable>
    where TDefinable : Definable
{
    IImplementation? Feature { get; }
}

interface IGenericProviderForType
{
    IEnumerable<IConversion> GetForcedConversions(TypeBase typeBase);
    IEnumerable<IConversion> GetForcedConversions(Pair typeBase);
}

interface IDeclarationProvider
{
    IEnumerable<SearchResult> GetDeclarations(TypeBase source);
    IEnumerable<IImplementation> GetDeclarations(ContextBase source);
}

sealed class GenericProviderForType<T> : DumpableObject, IGenericProviderForType
{
    readonly T Target;
    public GenericProviderForType(T target) => Target = target;

    IEnumerable<IConversion> IGenericProviderForType.GetForcedConversions(TypeBase source)
        => source.GetForcedConversions(Target);

    IEnumerable<IConversion> IGenericProviderForType.GetForcedConversions(Pair source)
        => source.GetForcedConversions(Target);
}

sealed class GenericProviderForDefinable<T> : DumpableObject, IDeclarationProvider
    where T : Definable
{
    readonly T Target;
    public GenericProviderForDefinable(T target) => Target = target;

    IEnumerable<SearchResult> IDeclarationProvider.GetDeclarations(TypeBase source)
        => source.GetDeclarations(Target);

    IEnumerable<IImplementation> IDeclarationProvider.GetDeclarations(ContextBase source)
        => source.GetDeclarations(Target);
}

sealed class MetaFunction : DumpableObject, IImplementation, IMeta
{
    readonly Func<Category, ResultCache, ContextBase, ValueSyntax?, Result> Function;

    public MetaFunction(Func<Category, ResultCache, ContextBase, ValueSyntax?, Result> function)
        => Function = function;

    IFunction? IEvalImplementation.Function => null;
    IValue? IEvalImplementation.Value => null;

    Result IMeta.GetResult
        (Category category, ResultCache left, SourcePart token, ContextBase contextBase, ValueSyntax? right)
        => Function(category, left, contextBase, right);

    IMeta IMetaImplementation.Function => this;
}

sealed class ContextMetaFunction : ContextMetaFeatureImplementation
{
    readonly Func<ContextBase, Category, ValueSyntax?, Result> Function;

    public ContextMetaFunction(Func<ContextBase, Category, ValueSyntax?, Result> function) => Function = function;

    protected override Result GetResult(Category category, ContextBase contextBase, ValueSyntax? right)
        => Function(contextBase, category, right);
}

sealed class ContextMetaFunctionFromSyntax
    : DumpableObject, IImplementation, IMeta
{
    [EnableDump]
    readonly ValueSyntax Definition;

    public ContextMetaFunctionFromSyntax(ValueSyntax definition) => Definition = definition;
    IFunction? IEvalImplementation.Function => null;
    IValue? IEvalImplementation.Value => null;

    Result IMeta.GetResult
    (
        Category category
        , ResultCache left
        , SourcePart token
        , ContextBase callContext
        , ValueSyntax? right
    )
        => callContext.GetResult(category, Definition.ReplaceArg(right));

    IMeta IMetaImplementation.Function => this;
}

interface IForcedConversionProvider<in TDestination>
{
    IEnumerable<IConversion> GetResult(TDestination destination);
}

interface IForcedConversionProviderForPointer<in TDestination>
{
    IEnumerable<IConversion?> GetResult(TDestination destination);
}

interface IChild<out TParent>
{
    [PublicAPI]
    TParent? Parent { get; }
}
