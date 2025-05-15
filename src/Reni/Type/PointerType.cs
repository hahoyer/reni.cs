using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Struct;
using Reni.TokenClasses;

namespace Reni.Type;

sealed class PointerType
    : TypeBase
        , IProxyType
        , IConversion
        , IReference
        , IChild<TypeBase>
        , ISymbolProvider<StableReference>
{
    [Node]
    [DisableDump]
    internal TypeBase ValueType { get; }

    readonly int Order;

    StableReferenceType StableReferenceType
        => this.CachedValue(() => new StableReferenceType(this));

    internal PointerType(TypeBase valueType)
    {
        Order = Closures.NextOrder++;
        ValueType = valueType;
        (!valueType.IsHollow).Assert(valueType.Dump);
        valueType.IsPointerPossible.Assert(valueType.Dump);
        StopByObjectIds(-10);
    }

    TypeBase IChild<TypeBase>.Parent => ValueType;

    int IContextReference.Order => Order;
    Result IConversion.Execute(Category category) => DereferenceResult(category);
    TypeBase IConversion.Source => this;

    IConversion IProxyType.Converter => this;
    IConversion IReference.Converter => this;
    bool IReference.IsWeak => true;

    IImplementation ISymbolProvider<StableReference>.Feature
        => Feature.Extension.Value(GetConversionToStableReference);

    [DisableDump]
    internal override Root? Root => ValueType.Root;

    internal override string DumpPrintText => "(" + ValueType.DumpPrintText + ")[Pointer]";

    [DisableDump]
    internal override CompoundView FindRecentCompoundView => ValueType.FindRecentCompoundView;

    [DisableDump]
    internal override IImplementation? CheckedFeature => ValueType.CheckedFeature;

    [DisableDump]
    internal override bool IsHollow => false;

    [DisableDump]
    internal override bool IsAligningPossible => false;

    [DisableDump]
    internal override bool IsPointerPossible => false;

    [DisableDump]
    protected override IEnumerable<IConversion> RawSymmetricConversions
        =>
            base.RawSymmetricConversions.Concat
                ([Feature.Extension.Conversion(DereferenceResult)]);

    protected override string GetNodeDump() => ValueType.NodeDump + "[Pointer]";

    internal override int? GetSmartArrayLength(TypeBase elementType)
        => ValueType.GetSmartArrayLength(elementType);

    protected override Size GetSize() => Root.DefaultRefAlignParam.RefSize;

    protected override ArrayType GetArrayForCache(int count, string optionsId)
        => ValueType.GetArray(count, optionsId);

    internal override IEnumerable<IConversion> GetForcedConversions<TDestination>(TDestination destination)
    {
        if(ValueType is IForcedConversionProviderForPointer<TDestination> provider)
            foreach(var feature in provider.GetResult(destination))
                yield return feature ?? throw new InvalidOperationException();

        foreach(var feature in base.GetForcedConversions(destination))
            yield return feature;
    }

    [DisableDump]
    protected override IEnumerable<IConversion> StripConversions
        => ValueType.StripConversionsFromPointer;

    [DisableDump]
    internal override IImplementation? FunctionDeclarationForType
        => ValueType.FunctionDeclarationForPointerType ?? base.FunctionDeclarationForType;

    internal override IEnumerable<SearchResult> GetDeclarations<TDefinable>(TDefinable? tokenClass)
        where TDefinable : class
    {
        if((ValueType as ISymbolProviderForPointer<TDefinable>)?.Feature is { } feature)
            return [SearchResult.Create(feature, this)];

        if(tokenClass != null 
           && (ValueType as IMultiSymbolProviderForPointer<TDefinable>)?.GetFeature(tokenClass) is { } multiFeature)
            return [SearchResult.Create(multiFeature, this)];

        return base.GetDeclarations(tokenClass);
    }

    protected override ResultCache GetDePointer(Category category)
        => ValueType
            .GetResult
            (
                category,
                () => ArgumentCode.GetDePointer(ValueType.Size),
                Closures.GetArgument
            );

    Result GetConversionToStableReference(Category category)
        => GetMutation(StableReferenceType) & category;

    Result DereferenceResult(Category category)
        => ValueType
            .Align
            .GetResult
            (
                category,
                () => ArgumentCode.GetDePointer(ValueType.Size).GetAlign(),
                Closures.GetArgument
            );
}