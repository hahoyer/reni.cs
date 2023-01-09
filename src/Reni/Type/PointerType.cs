using hw.DebugFormatter;
using hw.Helper;
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
        (StableReference tokenClass)
        => Feature.Extension.Value(ConvertToStableReference);

    [DisableDump]
    internal override Root Root => ValueType.Root;

    internal override string DumpPrintText => "(" + ValueType.DumpPrintText + ")[Pointer]";

    [DisableDump]
    internal override CompoundView FindRecentCompoundView => ValueType.FindRecentCompoundView;

    [DisableDump]
    internal override IImplementation CheckedFeature => ValueType.CheckedFeature;

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
                (new IConversion[] { Feature.Extension.Conversion(DereferenceResult) });

    protected override string GetNodeDump() => ValueType.NodeDump + "[Pointer]";

    internal override int? SmartArrayLength(TypeBase elementType)
        => ValueType.SmartArrayLength(elementType);

    protected override Size GetSize() => Root.DefaultRefAlignParam.RefSize;

    protected override ArrayType GetArrayForCache(int count, string optionsId)
        => ValueType.Array(count, optionsId);

    internal override IEnumerable<IConversion> GetForcedConversions<TDestination>
        (TDestination destination)
    {
        var provider = ValueType as IForcedConversionProviderForPointer<TDestination>;
        if(provider != null)
            foreach(var feature in provider.Result(destination))
                yield return feature;

        foreach(var feature in base.GetForcedConversions(destination))
            yield return feature;
    }

    [DisableDump]
    protected override IEnumerable<IConversion> StripConversions
        => ValueType.StripConversionsFromPointer;

    [DisableDump]
    internal override IImplementation FunctionDeclarationForType
        => ValueType.FunctionDeclarationForPointerType ?? base.FunctionDeclarationForType;

    internal override IEnumerable<SearchResult> Declarations<TDefinable>(TDefinable tokenClass)
    {
        var feature = (ValueType as ISymbolProviderForPointer<TDefinable>)?.Feature(tokenClass);
        if(feature == null)
            return base.Declarations(tokenClass);

        return new[]
        {
            SearchResult.Create(feature, this)
        };
    }

    protected override ResultCache DePointer(Category category)
        => ValueType
            .Result
            (
                category,
                () => ArgCode.DePointer(ValueType.Size),
                Closures.Arg
            );

    internal override Result ConvertToStableReference(Category category)
        => Mutation(StableReferenceType) & category;

    Result DereferenceResult(Category category)
        => ValueType
            .Align
            .Result
            (
                category,
                () => ArgCode.DePointer(ValueType.Size).Align(),
                Closures.Arg
            );
}