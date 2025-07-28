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
        (!valueType.OverView.IsHollow).Assert(valueType.Dump);
        valueType.OverView.IsPointerPossible.Assert(valueType.Dump);
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
    internal override Root Root => ValueType.Root;

    protected override string GetDumpPrintText() => "(" + ValueType.OverView.DumpPrintText + ")[Pointer]";

    internal override CompoundView FindRecentCompoundView() => ValueType.FindRecentCompoundView();

    internal override IImplementation? GetCheckedFeature() => ValueType.GetCheckedFeature();

    protected override bool GetIsHollow() => false;

    protected override bool GetIsAligningPossible() => false;

    protected override bool GetIsPointerPossible() => false;

    protected override IEnumerable<IConversion> GetSymmetricConversions() => base.GetSymmetricConversions().Concat
        ([Feature.Extension.Conversion(DereferenceResult)]);

    protected override IEnumerable<IGenericProviderForType> GetGenericProviders() => this.GetGenericProviders(base.GetGenericProviders());

    protected override string GetNodeDump() => ValueType.NodeDump + "[Pointer]";

    internal override int? GetSmartArrayLength(TypeBase elementType)
        => ValueType.GetSmartArrayLength(elementType);

    protected override Size GetSize() => Root.DefaultRefAlignParam.RefSize;

    protected override PointerType GetPointerForCache() => this;

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

    protected override IEnumerable<IConversion> GetStripConversions() => ValueType.Conversion.StripFromPointer;

    internal override IImplementation? GetFunctionDeclarationForType()
        => ValueType.GetFunctionDeclarationForPointerType() ?? base.GetFunctionDeclarationForType();

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
                () => Make.ArgumentCode.GetDePointer(ValueType.OverView.Size),
                Closures.GetArgument
            );

    Result GetConversionToStableReference(Category category)
        => GetMutation(StableReferenceType) & category;

    Result DereferenceResult(Category category)
        => ValueType
            .Make.Align
            .GetResult
            (
                category,
                () => Make.ArgumentCode.GetDePointer(ValueType.OverView.Size).GetAlign(),
                Closures.GetArgument
            );

    internal Result ConversionResult(Category category, ArrayType source)
    {
        var trace = ObjectId == -1 && category.HasCode();
        StartMethodDump(trace, category, source);
        try
        {
            return ReturnMethodDump(source.Make.Pointer.GetMutation(this) & category);
        }
        finally
        {
            EndMethodDump();
        }
    }

}