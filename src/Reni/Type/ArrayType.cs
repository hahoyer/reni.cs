using hw.DebugFormatter;
using hw.Helper;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.Type;

sealed class ArrayType
    : TypeBase
        , ISymbolProviderForPointer<DumpPrintToken>
        , ISymbolProviderForPointer<ConcatArrays>
        , ISymbolProviderForPointer<TextItem>
        , ISymbolProviderForPointer<ToNumberOfBase>
        , ISymbolProviderForPointer<Mutable>
        , ISymbolProviderForPointer<ArrayReference>
        , ISymbolProviderForPointer<Count>
        , IForcedConversionProviderForPointer<ArrayReferenceType>
        , IRepeaterType
        , IChild<TypeBase>
{
    internal sealed class Options : DumpableObject
    {
        internal static readonly string DefaultOptionsId = Create().Data.Id;

        public Flag IsMutable { get; }
        public Flag IsTextItem { get; }
        Flags Data { get; }
        public string DumpPrintText => Data.DumpPrintText;

        Options(string optionsId)
        {
            Data = new(optionsId);
            IsMutable = Data.Register("mutable");
            IsTextItem = Data.Register("text_item");
            Data.Align();
            Data.IsValid.Assert();
        }

        protected override string GetNodeDump() => DumpPrintText;

        public static Options Create(string optionsId = null) => new(optionsId);
    }

    [DisableDump]
    internal TypeBase ElementType { get; }

    internal int Count { get; }

    [Node]
    [SmartNode]
    readonly ValueCache<RepeaterAccessType> RepeaterAccessTypeCache;

    [Node]
    [SmartNode]
    readonly ValueCache<NumberType> NumberCache;

    Options OptionsValue { get; }

    [DisableDump]
    RepeaterAccessType AccessType => RepeaterAccessTypeCache.Value;

    [DisableDump]
    internal bool IsMutable => OptionsValue.IsMutable.Value;

    [DisableDump]
    internal bool IsTextItem => OptionsValue.IsTextItem.Value;

    [DisableDump]
    internal NumberType Number => NumberCache.Value;

    [DisableDump]
    ArrayType NoTextItem => ElementType.Array(Count, OptionsValue.IsTextItem.SetTo(false));

    [DisableDump]
    internal ArrayType TextItem => ElementType.Array(Count, OptionsValue.IsTextItem.SetTo(true));

    [DisableDump]
    internal ArrayType Mutable => ElementType.Array(Count, OptionsValue.IsMutable.SetTo(true));

    IEnumerable<string> InternalDeclarationOptions
    {
        get
        {
            NotImplementedMethod();
            return null;
        }
    }

    TypeBase ElementAccessType => ElementType.TypeForArrayElement;

    [DisableDump]
    TypeBase IndexType => Root.BitType.Number(IndexSize.ToInt());

    Size IndexSize => Size.AutoSize(Count).Align(Root.DefaultRefAlignParam.AlignBits);

    public ArrayType(TypeBase elementType, int count, string optionsId)
    {
        ElementType = elementType;
        Count = count;
        OptionsValue = Options.Create(optionsId);
        (count > 0).Assert();
        (elementType.CheckedReference == null).Assert();
        (!elementType.IsHollow).Assert();
        RepeaterAccessTypeCache = new(() => new(this));
        NumberCache = new(() => new(this));
    }

    TypeBase IChild<TypeBase>.Parent => ElementType;

    IEnumerable<IConversion> IForcedConversionProviderForPointer<ArrayReferenceType>.Result
        (ArrayReferenceType destination)
        => ForcedConversion(destination).NullableToArray();

    TypeBase IRepeaterType.ElementType => ElementType;
    TypeBase IRepeaterType.IndexType => Root.BitType.Number(IndexSize.ToInt());
    bool IRepeaterType.IsMutable => IsMutable;

    IImplementation ISymbolProviderForPointer<ArrayReference>.
        Feature
        (ArrayReference tokenClass)
        => Feature.Extension.Value(ReferenceResult);

    IImplementation ISymbolProviderForPointer<ConcatArrays>.
        Feature(ConcatArrays tokenClass)
        =>
            Feature.Extension.FunctionFeature
            (
                (category, objectReference, argsType) =>
                    ConcatArraysResult
                    (
                        category,
                        objectReference,
                        argsType,
                        OptionsValue.IsMutable.SetTo(tokenClass.IsMutable)),
                this);

    IImplementation ISymbolProviderForPointer<Count>.Feature
        (Count tokenClass)
        => Feature.Extension.MetaFeature(CountResult);

    IImplementation ISymbolProviderForPointer<DumpPrintToken>.
        Feature
        (DumpPrintToken tokenClass)
        =>
            OptionsValue.IsTextItem.Value
                ? Feature.Extension.Value(DumpPrintTokenResult)
                : Feature.Extension.Value(DumpPrintTokenArrayResult);

    IImplementation ISymbolProviderForPointer<Mutable>.Feature
        (Mutable tokenClass)
        => Feature.Extension.Value(MutableResult);

    IImplementation ISymbolProviderForPointer<TextItem>.Feature
        (TextItem tokenClass)
        => Feature.Extension.Value(TextItemResult);


    IImplementation ISymbolProviderForPointer<ToNumberOfBase>.
        Feature
        (ToNumberOfBase tokenClass)
        =>
            OptionsValue.IsTextItem.Value
                ? Feature.Extension.MetaFeature(ToNumberOfBaseResult)
                : null;

    [DisableDump]
    internal override bool IsHollow => Count == 0 || ElementType.IsHollow;

    internal override string DumpPrintText
        => "(" + ElementType.DumpPrintText + ")*" + Count + OptionsValue.DumpPrintText;

    [DisableDump]
    internal override Size SimpleItemSize
        =>
            OptionsValue.IsTextItem.Value
                ? ElementType.SimpleItemSize ?? Size
                : base.SimpleItemSize;

    [DisableDump]
    internal override IImplementation FunctionDeclarationForPointerType
        => Feature.Extension.FunctionFeature(ElementAccessResult);

    internal override IEnumerable<string> DeclarationOptions
        => base.DeclarationOptions.Concat(InternalDeclarationOptions);

    internal override int? SmartArrayLength(TypeBase elementType)
        => ElementType.IsConvertible(elementType)? Count : base.SmartArrayLength(elementType);

    protected override Size GetSize() => ElementType.Size * Count;

    internal override Result Copier(Category category)
        => ElementType.ArrayCopier(category);

    internal override Result Cleanup(Category category)
        => ElementType.ArrayCleanup(category);

    [DisableDump]
    protected override IEnumerable<IConversion> StripConversions
    {
        get { yield return Feature.Extension.Conversion(NoTextItemResult); }
    }

    internal override Result ConstructorResult(Category category, TypeBase argsType)
    {
        if(category == Category.None)
            return null;

        if(argsType == Root.VoidType)
            return Result(category, () => CodeBase.BitsConst(Size, BitsConst.Convert(0)));

        var function = argsType as IFunction;
        if(function != null)
            return ConstructorResult(category, function);

        return base.ConstructorResult(category, argsType);
    }

    [DisableDump]
    internal override Root Root => ElementType.Root;

    protected override string GetNodeDump()
        => ElementType.NodeDump + "*" + Count + OptionsValue.NodeDump;

    protected override CodeBase DumpPrintCode() => ArgCode.DumpPrintText(SimpleItemSize);

    internal ArrayReferenceType Reference(bool isForceMutable)
        => ElementType.ArrayReference(ArrayReferenceType.Options.ForceMutable(isForceMutable));

    Result NoTextItemResult(Category category) => ResultFromPointer(category, NoTextItem);
    Result TextItemResult(Category category) => ResultFromPointer(category, TextItem);
    Result MutableResult(Category category) => ResultFromPointer(category, Mutable);

    Result ReferenceResult(Category category)
        => Reference(IsMutable).Result(category, ObjectResult);

    Result ConstructorResult(Category category, IFunction function)
    {
        var indexType = BitType
            .Number(BitsConst.Convert(Count).Size.ToInt())
            .Align;
        var constructorResult = function.Result(category | Category.Type, indexType);
        var elements = Count
                .Select(i => ElementConstructorResult(category, constructorResult, i, indexType))
                .Aggregate((c, n) => n + c)
            ?? Root.VoidType.Result(category);
        return Result(category, elements);
    }

    Result ElementConstructorResult
        (Category category, Result elementConstructorResult, int i, TypeBase indexType)
    {
        var resultForArg = indexType
            .Result
                (category | Category.Type, () => CodeBase.BitsConst(indexType.Size, BitsConst.Convert(i)));
        return elementConstructorResult
                .ReplaceArg(resultForArg)
                .Conversion(ElementAccessType)
                .Conversion(ElementType)
            & category;
    }

    Result ConcatArraysResult
    (
        Category category,
        IContextReference objectReference,
        TypeBase argsType,
        string argsOptions
    )
    {
        var oldElementsResult = Pointer
            .Result(category | Category.Type, objectReference).DereferenceResult;

        var isElementArg = argsType.IsConvertible(ElementAccessType);
        var newCount = isElementArg? 1 : argsType.ArrayLength(ElementAccessType);
        var newElementsResultRaw
            = isElementArg
                ? argsType.Conversion(category | Category.Type, ElementAccessType)
                : argsType.Conversion(category | Category.Type, ElementType.Array(newCount, argsOptions));

        var newElementsResult = newElementsResultRaw.AutomaticDereferencedAlignedResult();
        var result = ElementType
            .Array(Count + newCount, argsOptions)
            .Result(category, newElementsResult + oldElementsResult);
        return result;
    }

    Result DumpPrintTokenArrayResult(Category category)
    {
        var result = Root.ConcatPrintResult(category, Count, DumpPrintResult);
        if(category.HasCode())
            result.Code = CodeBase.DumpPrintText
                    ("<<" + (OptionsValue.IsMutable.Value? ":=" : ""))
                + result.Code;
        return result;
    }

    Result DumpPrintResult(Category category, int position) => ElementType
        .SmartPointer
        .GenericDumpPrintResult(category)
        .ReplaceAbsolute
        (
            ElementType.Pointer.CheckedReference,
            c => ReferenceResult(c).AddToReference(() => ElementType.Size * position)
        );

    Result ElementAccessResult(Category category, TypeBase right)
        => AccessType.Result(category, ObjectResult(category), right);

    Result ToNumberOfBaseResult
        (Category category, ResultCache left, ContextBase context, ValueSyntax right)
    {
        var target = (left & Category.All)
            .AutomaticDereferencedAlignedResult()
            .Evaluate(context.RootContext.ExecutionContext)
            .ToString(ElementType.Size);
        var conversionBase = right.Evaluate(context).ToInt32();
        (conversionBase >= 2).Assert(conversionBase.ToString);
        var result = BitsConst.Convert(target, conversionBase);
        return Root.BitType.Result(category, result).Align;
    }

    Result CountResult
        (Category category, ResultCache left, ContextBase context, ValueSyntax right)
    {
        (right == null).Assert();
        return IndexType.Result
            (category, () => CodeBase.BitsConst(IndexSize, BitsConst.Convert(Count)));
    }

    IConversion ForcedConversion(ArrayReferenceType destination)
    {
        if(!HasForcedConversion(destination))
            return null;

        return Feature.Extension.Conversion
            (category => destination.ConversionResult(category, this), SmartPointer);
    }

    bool HasForcedConversion(ArrayReferenceType destination)
    {
        if(destination.IsMutable && !OptionsValue.IsMutable.Value)
            return false;

        if(ElementType == destination.ValueType)
            return true;

        NotImplementedMethod(destination);
        return false;
    }
}