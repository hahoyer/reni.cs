using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Numeric;
using Reni.TokenClasses;

namespace Reni.Type;

sealed class ArrayReferenceType
    : TypeBase
        , ISymbolProviderForPointer<DumpPrintToken>
        , ISymbolProviderForPointer<Mutable>
        , ISymbolProviderForPointer<EnableReinterpretation>
        , ISymbolProviderForPointer<Plus>
        , ISymbolProviderForPointer<Minus>
        , IForcedConversionProvider<ArrayReferenceType>
        , IRepeaterType
        , IChild<TypeBase>
{
    internal sealed class Options : DumpableObject
    {
        internal Flag IsMutable { get; }
        internal Flag IsForceMutable { get; }
        internal Flag IsEnableReinterpretation { get; }
        Flags Data { get; }
        public string DumpPrintText => Data.DumpPrintText;

        Options(string optionsId)
        {
            Data = new(optionsId);
            IsForceMutable = Data.Register("force_mutable");
            IsMutable = Data.Register("mutable");
            IsEnableReinterpretation = Data.Register("enable_reinterpretation");
            Data.Align();
            Data.IsValid.Assert();
        }

        protected override string GetNodeDump() => DumpPrintText;

        internal static Options Create(string optionsId) => new(optionsId);

        internal static string ForceMutable(bool value)
            => Create("").IsForceMutable.SetTo(value);
    }

    [DisableDump]
    internal TypeBase ValueType { get; }

    [UsedImplicitly]
    readonly int Order;

    readonly ValueCache<RepeaterAccessType> RepeaterAccessTypeCache;
    Options OptionsValue { get; }

    internal string DumpOptions => OptionsValue.DumpPrintText;

    [DisableDump]
    RepeaterAccessType AccessType => RepeaterAccessTypeCache.Value;

    static IEnumerable<string> InternalDeclarationOptions
    {
        get
        {
            yield return DumpPrintToken.TokenId;
            yield return TokenClasses.Mutable.TokenId;
            yield return TokenClasses.EnableReinterpretation.TokenId;
            yield return Plus.TokenId;
            yield return Minus.TokenId;
        }
    }

    [DisableDump]
    internal bool IsMutable => OptionsValue.IsMutable.Value;

    [DisableDump]
    internal ArrayReferenceType Mutable
        => ValueType.GetArrayReference(OptionsValue.IsMutable.SetTo(true));

    [DisableDump]
    internal ArrayReferenceType EnableReinterpretation
        => ValueType.GetArrayReference(OptionsValue.IsEnableReinterpretation.SetTo(true));

    internal ArrayReferenceType(TypeBase valueType, string optionsId)
    {
        Order = Closures.NextOrder++;
        OptionsValue = Options.Create(optionsId);
        RepeaterAccessTypeCache = new(() => new(this));
        ValueType = valueType;
        (!valueType.OverView.IsHollow).Assert(valueType.Dump);
        (valueType.Make.TagTargetType is not PointerType).Assert(valueType.Dump);

        StopByObjectIds(-10);
    }

    TypeBase IChild<TypeBase>.Parent => ValueType;

    IEnumerable<IConversion> IForcedConversionProvider<ArrayReferenceType>.GetResult
        (ArrayReferenceType destination)
        => ForcedConversion(destination).NullableToArray();

    TypeBase IRepeaterType.ElementType => ValueType;
    TypeBase IRepeaterType.IndexType => Root.BitType.Number(OverView.Size.ToInt());
    Root IRepeaterType.Root => Root;
    bool IRepeaterType.IsMutable => OptionsValue.IsForceMutable.Value;

    IImplementation ISymbolProviderForPointer<DumpPrintToken>.Feature => Feature.Extension.Value(GetDumpPrintTokenResult);

    IImplementation ISymbolProviderForPointer<EnableReinterpretation>.Feature
        => Feature.Extension.Value(EnableReinterpretationResult);

    IImplementation ISymbolProviderForPointer<Minus>.Feature
        => Feature.Extension.FunctionFeature(MinusResult);

    IImplementation ISymbolProviderForPointer<Mutable>.Feature
        => Feature.Extension.Value(MutableResult);

    IImplementation ISymbolProviderForPointer<Plus>.Feature
        => Feature.Extension.FunctionFeature(PlusResult);

    [DisableDump]
    internal override Root Root => ValueType.Root;

    protected override string GetDumpPrintText()
        => "(" + ValueType.OverView.DumpPrintText + ")reference" + OptionsValue.DumpPrintText;

    protected override bool GetIsHollow() => false;

    protected override bool GetIsAligningPossible() => false;

    [DisableDump]
    internal override IEnumerable<string> DeclarationOptions
        => base.DeclarationOptions.Concat(InternalDeclarationOptions);

    protected override IEnumerable<IGenericProviderForType> GetGenericProviders() 
        => this.GetGenericProviders(base.GetGenericProviders());

    //todo: Why textitem-tag is not relevant here? 
    internal override Size GetTextItemSize() => ValueType.OverView.Size;

    [DisableDump]
    protected override CodeBase DumpPrintCode => Make.ArgumentCode.GetDumpPrintText(GetTextItemSize());

    protected override string GetNodeDump()
        => ValueType.NodeDump + "[array_reference]" + OptionsValue.NodeDump;

    protected override Size GetSize() => ValueType.Make.Pointer.OverView.Size;

    internal override IImplementation GetFunctionDeclarationForPointerType()
        => Feature.Extension.FunctionFeature(AccessResult);


    Result MutableResult(Category category)
    {
        OptionsValue.IsForceMutable.Value.Assert();
        return GetResultFromPointer(category, Mutable);
    }

    Result EnableReinterpretationResult(Category category)
        => GetResultFromPointer(category, EnableReinterpretation);

    IConversion? ForcedConversion(ArrayReferenceType destination)
        =>
            HasForcedConversion(destination)
                ? Feature.Extension.Conversion
                    (category => destination.ConversionResult(category, this), this)
                : null;

    bool HasForcedConversion(ArrayReferenceType destination)
    {
        if(this == destination)
            return true;

        if(destination.IsMutable && !IsMutable)
            return false;

        if(ValueType == destination.ValueType)
            return true;

        if(ValueType == destination.ValueType)
            NotImplementedMethod(destination);

        return OptionsValue.IsEnableReinterpretation.Value;
    }

    Result ConversionResult(Category category, ArrayReferenceType source)
        => source.GetMutation(this) & category;

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

    Result AccessResult(Category category, TypeBase right)
    {
        var leftResult = GetObjectResult(category).DereferenceResult;
        return AccessType
            .GetResult(category, leftResult, right);
    }

    Result PlusResult(Category category, TypeBase right)
    {
        var codeAndClosures = AccessResult(category, right).DereferenceResult;
        return GetResult(category, codeAndClosures);
    }

    Result MinusResult(Category category, TypeBase right)
    {
        NotImplementedMethod(category, right);
        return null!;
    }
}