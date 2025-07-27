using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Numeric;
using Reni.TokenClasses;

namespace Reni.Type;

sealed class NumberType
    : Child<ArrayType>
        , ISymbolProviderForPointer<DumpPrintToken>
        , IMultiSymbolProviderForPointer<Operation>
        , ISymbolProviderForPointer<TokenClasses.EnableCut>
        , ISymbolProviderForPointer<Negate>
        , ISymbolProviderForPointer<TextItem>
        , IForcedConversionProvider<NumberType>
{
    internal interface IOperation
    {
        [DisableDump]
        string Name { get; }
    }

    internal interface ITransformation
    {
        int Signature(int objectBitCount, int argsBitCount);
    }

    static readonly Minus MinusOperation = new();
    readonly ValueCache<Result> ZeroResult;
    NumberType ZeroType => (NumberType)ZeroResult.Value.Type;

    [EnableDump]
    internal int Bits => Size.ToInt();

    static IEnumerable<string> InternalDeclarationOptions
        => new[]
            {
                DumpPrintToken.TokenId, TokenClasses.EnableCut.TokenId, Negate.TokenId, TextItem.TokenId
            }
            .Concat(Extension.GetTokenIds<Operation>());

    public NumberType(ArrayType parent)
        : base(parent)
    {
        ZeroResult = new(GetZeroResult);
        StopByObjectIds();
    }

    IEnumerable<IConversion> IForcedConversionProvider<NumberType>.GetResult(NumberType destination)
    {
        if(Bits <= destination.Bits)
            yield return
                Feature.Extension.Conversion
                    (category => destination.FlatConversion(category, this), this);
    }

    IImplementation IMultiSymbolProviderForPointer<Operation>.GetFeature(Operation tokenClass)
        => Feature.Extension.FunctionFeature(OperationResult, tokenClass);

    IImplementation ISymbolProviderForPointer<DumpPrintToken>.Feature
        => Feature.Extension.Value(GetDumpPrintTokenResult, this);

    IImplementation ISymbolProviderForPointer<TokenClasses.EnableCut>.Feature
        => Feature.Extension.Value(EnableCutTokenResult);

    IImplementation ISymbolProviderForPointer<Negate>.Feature
        => Feature.Extension.Value(NegationResult);

    IImplementation ISymbolProviderForPointer<TextItem>.Feature
        => Feature.Extension.Value(TextItemResult);

    [DisableDump]
    internal override Root Root => Parent.Root;

    [DisableDump]
    internal override bool IsHollow => Parent.IsHollow;

    [DisableDump]
    internal override string DumpPrintText => "number(bits:" + Bits + ")";

    [DisableDump]
    internal override bool IsCuttingPossible => true;

    [DisableDump]
    protected override IEnumerable<IGenericProviderForType> GenericList
        => this.GenericListFromType(base.GenericList);

    [DisableDump]
    internal override IEnumerable<string> DeclarationOptions
        => base.DeclarationOptions.Concat(InternalDeclarationOptions);

    internal override IEnumerable<IConversion> GetCutEnabledConversion(NumberType destination)
    {
        yield return
            Feature.Extension.Conversion
                (category => CutEnabledBitCountConversion(category, destination), Make.EnableCut);
    }

    protected override Result ParentConversionResult(Category category)
        => GetMutation(Parent) & category;

    protected override Size GetSize() => Parent.Size;

    [DisableDump]
    protected override CodeBase DumpPrintCode => Make.Align.Make.ArgumentCode.GetDumpPrintNumber(Make.Align.Size);

    internal override object GetDataValue(BitsConst data) => data.ToInt32();

    Result GetZeroResult() => Root
        .BitType
        .Number(1)
        .GetResult(Category.All, () => Code.Extension.GetCode(BitsConst.Convert(0)));

    Result TextItemResult(Category category) => Parent
        .TextItem.Make.Pointer
        .GetResult
        (
            category,
            Parent.Make.Pointer
                .GetResult(category, GetObjectResult(category | Category.Type)));

    Result NegationResult(Category category)
        => ZeroType
            .OperationResult(category, MinusOperation, this)
            .ReplaceAbsolute
            (
                ZeroType.Make.ForcedReference
                , c => ZeroResult.Value.LocalReferenceResult & c
            )
            .ReplaceArguments(GetObjectResult);

    Result EnableCutTokenResult(Category category)
        => Make
            .EnableCut.Make.Pointer
            .GetResult(category | Category.Type, GetObjectResult(category));

    Result OperationResult(Category category, TypeBase right, IOperation operation)
    {
        var destination = ConversionService
            .FindPathDestination<NumberType>(right)
            .SingleOrDefault();

        if(destination != null)
            return OperationResult(category, operation, destination)
                .ReplaceArguments(c => right.GetConversion(c, destination.Make.Pointer));

        NotImplementedMethod(category, right, operation);
        return null!;
    }

    Result OperationResult(Category category, IOperation operation, NumberType right)
    {
        var transformation = operation as ITransformation;
        var resultType = transformation == null
            ? (TypeBase)Root.BitType
            : Root.BitType.Number(transformation.Signature(Bits, right.Bits));
        return OperationResult(category, resultType, operation.Name, right);
    }

    Result OperationResult(Category category, TypeBase resultType, string operationName, NumberType right)
    {
        var result = resultType.GetResult
        (
            category,
            () => OperationCode(resultType.Size, operationName, right),
            Closures.GetArgument
        );

        var leftResult = GetObjectResult(category | Category.Type)
            .GetConversion(Make.Align);
        var rightResult = right.Make.Pointer.GetArgumentResult(category | Category.Type).GetConversion(right.Make.Align);
        return result.ReplaceArguments((leftResult + rightResult)!);
    }

    CodeBase OperationCode(Size resultSize, string token, TypeBase right)
    {
        (!(right is PointerType)).Assert();
        return Make.Align
            .GetPair(right.Make.Align)
            .Make.ArgumentCode
            .GetNumberOperation(token, resultSize, Make.Align.Size, right.Make.Align.Size);
    }

    Result FlatConversion(Category category, NumberType source)
    {
        if(Bits == source.Bits)
            return GetArgumentResult(category | Category.Type);

        return GetResult
        (
            category,
            () => source.Make.ArgumentCode.GetBitCast(Size),
            Closures.GetArgument
        );
    }

    Result CutEnabledBitCountConversion(Category category, NumberType destination)
    {
        if(Bits == destination.Bits)
            return Make.EnableCut.GetMutation(this) & category;

        return destination
            .GetResult
            (
                category,
                () => Make.EnableCut.Make.ArgumentCode.GetBitCast(destination.Size),
                Closures.GetArgument
            );
    }
}
