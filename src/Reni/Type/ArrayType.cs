using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Struct;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.Type;

sealed class ArrayType
    : TypeBase
        , ISymbolProviderForPointer<DumpPrintToken>
        , ISymbolProviderForPointer<ConcatArrays>
        , ISymbolProviderForPointer<MutableConcatArrays>
        , ISymbolProviderForPointer<TextItem>
        , ISymbolProviderForPointer<Mutable>
        , ISymbolProviderForPointer<ArrayReference>
        , ISymbolProviderForPointer<Count>
        , IForcedConversionProviderForPointer<ArrayReferenceType>
        , IForcedConversionProviderForPointer<PointerType>
        , IRepeaterType
        , IChild<TypeBase>
{
    internal sealed class Options : DumpableObject
    {
        internal static readonly string DefaultOptionsId = Create().Data.Id;

        public Flag IsMutable { get; }
        Flags Data { get; }
        public string DumpPrintText => Data.DumpPrintText;
        internal string NameDump => DumpPrintText;

        Options(string optionsId)
        {
            Data = new(optionsId);
            IsMutable = Data.Register("mutable");
            Data.Align();
            Data.IsValid.Assert();
        }

        protected override string GetNodeDump() => DumpPrintText;

        public static Options Create(string optionsId = "") => new(optionsId);
    }

    [DisableDump]
    [UsedImplicitly]
    internal override TypeBase ElementType { get; }

    [UsedImplicitly]
    [DisableDump]
    internal int Count { get; }

    [Node]
    [SmartNode]
    readonly ValueCache<RepeaterAccessType> RepeaterAccessTypeCache;

    [Node]
    [SmartNode]
    readonly ValueCache<NumberType> NumberCache;

    [Node]
    [SmartNode]
    readonly ValueCache<TextItemType> TextItemCache;

    Options OptionsValue { get; }

    [DisableDump]
    RepeaterAccessType AccessType => RepeaterAccessTypeCache.Value;

    [DisableDump]
    [UsedImplicitly]
    internal bool IsMutable => OptionsValue.IsMutable.Value;

    [DisableDump]
    internal NumberType Number => NumberCache.Value;

    [DisableDump]
    internal TextItemType TextItem => TextItemCache.Value;

    [DisableDump]
    internal ArrayType Mutable => ElementType.GetArray(Count, OptionsValue.IsMutable.SetTo(true));

    [DisableDump]
    IEnumerable<string> InternalDeclarationOptions
    {
        get
        {
            (Count > 0).Assert();
            return [];
        }
    }

    TypeBase ElementAccessType => ElementType.Make.TypeForArrayElement;

    [DisableDump]
    TypeBase IndexType => Root.BitType.Number(IndexSize.ToInt());

    Size IndexSize => Size.GetAutoSize(Count).GetAlign(Root.DefaultRefAlignParam.AlignBits);

    public ArrayType(TypeBase elementType, int count, string optionsId)
    {
        ElementType = elementType;
        Count = count;
        OptionsValue = Options.Create(optionsId);
        //(count > 0).Assert();
        (elementType.Make.CheckedReference == null).Assert();
        //(!elementType.IsHollow).Assert();
        RepeaterAccessTypeCache = new(() => new(this));
        NumberCache = new(() => new(this));
        TextItemCache = new(() => new(this));
        StopByObjectIds();
    }

    TypeBase IChild<TypeBase>.Parent => ElementType;

    IEnumerable<IConversion?> IForcedConversionProviderForPointer<ArrayReferenceType>
        .GetResult(ArrayReferenceType destination)
        => ForcedConversion(destination).NullableToArray();

    IEnumerable<IConversion?> IForcedConversionProviderForPointer<PointerType>
        .GetResult(PointerType destination)
        => ForcedConversion(destination).NullableToArray();

    TypeBase IRepeaterType.ElementType => ElementType;
    TypeBase IRepeaterType.IndexType => Root.BitType.Number(IndexSize.ToInt());
    bool IRepeaterType.IsMutable => IsMutable;
    Root IRepeaterType.Root => Root;

    IImplementation ISymbolProviderForPointer<ArrayReference>.Feature
        => Feature.Extension.Value(ReferenceResult);

    IImplementation ISymbolProviderForPointer<ConcatArrays>.Feature
        => Feature.Extension.FunctionFeature
        (
            (category, objectReference, argumentsType) =>
                ConcatArraysResult
                (
                    category,
                    objectReference,
                    argumentsType,
                    OptionsValue.IsMutable.SetTo(false)),
            this);

    IImplementation ISymbolProviderForPointer<Count>.Feature
        => Feature.Extension.MetaFeature(CountResult);

    IImplementation ISymbolProviderForPointer<DumpPrintToken>.Feature
        => Feature.Extension.Value(DumpPrintTokenArrayResult);

    IImplementation ISymbolProviderForPointer<Mutable>.Feature
        => Feature.Extension.Value(MutableResult);

    IImplementation ISymbolProviderForPointer<MutableConcatArrays>.Feature
        => Feature.Extension.FunctionFeature
        (
            (category, objectReference, argumentsType) =>
                ConcatArraysResult
                (
                    category,
                    objectReference,
                    argumentsType,
                    OptionsValue.IsMutable.SetTo(true)),
            this);

    IImplementation ISymbolProviderForPointer<TextItem>.Feature
        => Feature.Extension.Value(TextItemResult);


    protected override bool GetIsHollow() => Count == 0 || ElementType.OverView.IsHollow;

    protected override string GetDumpPrintText()
        => "(" + ElementType.OverView.DumpPrintText + ")*" + Count + OptionsValue.DumpPrintText;
    internal override CompoundView FindRecentCompoundView() => ElementType.FindRecentCompoundView();

    internal override IImplementation GetFunctionDeclarationForPointerType()
        => Feature.Extension.FunctionFeature(ElementAccessResult);

    [DisableDump]
    internal override IEnumerable<string> DeclarationOptions
        => base.DeclarationOptions.Concat(InternalDeclarationOptions);

    internal override int? GetSmartArrayLength(TypeBase elementType)
        => ElementType.IsConvertible(elementType)? Count : base.GetSmartArrayLength(elementType);

    protected override Size GetSize() => ElementType.OverView.Size * Count;

    internal override Result GetCopier(Category category)
        => ElementType.GetArrayCopier(category);

    internal override Result GetCleanup(Category category)
        => ElementType.GetArrayCleanup(category);

    internal override Result GetConstructorResult(Category category, TypeBase argumentsType)
    {
        if(category == Category.None)
            return new(Category.None);

        if(argumentsType == Root.VoidType)
            return GetResult(category, () => BitsConst.Convert(0).GetCode(OverView.Size));

        if(argumentsType is IFunction function)
            return ConstructorResult(category, function);

        return base.GetConstructorResult(category, argumentsType);
    }

    [DisableDump]
    internal override Root Root => ElementType.Root;

    protected override string GetNodeDump()
        => ElementType.NameDump + "*" + Count + OptionsValue.NameDump;

    protected override string GetNameDump()
        => ElementType.NameDump + "*" + Count + OptionsValue.NameDump;


    [DisableDump]
    protected override CodeBase DumpPrintCode
        => Make.ArgumentCode.GetDumpPrintText(ElementType.OverView.Size);

    internal ArrayReferenceType Reference(bool isForceMutable)
        => ElementType.GetArrayReference(ArrayReferenceType.Options.ForceMutable(isForceMutable));

    Result TextItemResult(Category category) => GetResultFromPointer(category, TextItem);
    Result MutableResult(Category category) => GetResultFromPointer(category, Mutable);

    Result ReferenceResult(Category category)
        => Reference(IsMutable).GetResult(category, GetObjectResult);

    Result ConstructorResult(Category category, IFunction function)
    {
        var indexType = Root.BitType
            .Number(BitsConst.Convert(Count).Size.ToInt())
            .Make.Align;
        var constructorResult = function.GetResult(category | Category.Type, indexType);
        var elements = Count
                .Select(i => ElementConstructorResult(category, constructorResult, i, indexType))
                .Aggregate(default(Result), (c, n) => n + c)
            ?? Root.VoidType.GetResult(category);
        return GetResult(category, elements);
    }

    Result ElementConstructorResult(Category category, Result elementConstructorResult, int i, TypeBase indexType)
    {
        var resultForArg = indexType
            .GetResult
                (category | Category.Type, () => BitsConst.Convert(i).GetCode(indexType.OverView.Size));
        return elementConstructorResult
                .ReplaceArguments(resultForArg)
                .GetConversion(ElementAccessType)
                .GetConversion(ElementType)
            & category;
    }

    Result ConcatArraysResult
    (
        Category category,
        IContextReference objectReference,
        TypeBase argumentsType,
        string? options
    )
    {
        var oldElementsResult = Make.Pointer
            .GetResult(category | Category.Type, objectReference)
            .Dereference;

        var newCount = argumentsType.GetArrayLength(ElementAccessType);
        var isElementArgument = newCount == 1 && argumentsType.IsConvertible(ElementAccessType);
        var newElementsResultRaw
            = isElementArgument
                ? argumentsType.GetConversion(category | Category.Type, ElementAccessType)
                : argumentsType.GetConversion(category | Category.Type, ElementType.GetArray(newCount, options));

        var newElementsResult = newElementsResultRaw.AutomaticDereferencedAligned;

        return ElementType
            .GetArray(Count + newCount, options)
            .GetResult(category, (newElementsResult + oldElementsResult)!);
    }

    Result DumpPrintTokenArrayResult(Category category)
    {
        var result = Root.ConcatPrintResult(category, Count, DumpPrintResult);
        if(category.HasCode)
            //todo: replace strings by TokenId
            result.Code = ("<<" + (OptionsValue.IsMutable.Value? ":=" : "")).DumpPrintTextCode
                + result.Code;
        return result;
    }

    Result DumpPrintResult(Category category, int position)
        => (ElementType.OverView.IsHollow? ElementType : ElementType.Make.Pointer)
            .GetGenericDumpPrintResult(category)
            .ReplaceAbsolute
            (
                ElementType.Make.Pointer.Make.CheckedReference!,
                c => ReferenceResult(c).AddToReference(() => ElementType.OverView.Size * position)
            );

    Result ElementAccessResult(Category category, TypeBase right)
        => AccessType.GetResult(category, GetObjectResult(category), right);

    Result CountResult(Category category, ResultCache left, ContextBase context, ValueSyntax? right)
        => IndexType.GetResult(category, () => BitsConst.Convert(Count).GetCode(IndexSize));

    IConversion? ForcedConversion(ArrayReferenceType destination)
    {
        if(!HasForcedConversion(destination))
            return null;

        return Feature.Extension.Conversion
            (category => destination.ConversionResult(category, this), GetIsHollow()? this : Make.Pointer);
    }

    IConversion? ForcedConversion(PointerType destination)
        => HasForcedConversion(destination)
            ? Feature.Extension.Conversion
            (
                category => destination.ConversionResult(category, this)
                , GetIsHollow()? this : Make.Pointer
            )
            : null;

    bool HasForcedConversion(ArrayReferenceType destination)
    {
        if(destination.IsMutable && !OptionsValue.IsMutable.Value)
            return false;

        if(ElementType == destination.ValueType)
            return true;

        NotImplementedMethod(destination);
        return false;
    }

    bool HasForcedConversion(PointerType destination) => ElementType == destination.ValueType;
}
