using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
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
        , ISymbolProviderForPointer<Operation>
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
        => ZeroResult = new(GetZeroResult);

    IEnumerable<IConversion> IForcedConversionProvider<NumberType>.Result(NumberType destination)
    {
        if(Bits <= destination.Bits)
            yield return
                Feature.Extension.Conversion
                    (category => destination.FlatConversion(category, this), this);
    }

    IImplementation ISymbolProviderForPointer<DumpPrintToken>.Feature(DumpPrintToken tokenClass)
        => Feature.Extension.Value(DumpPrintTokenResult, this);

    IImplementation ISymbolProviderForPointer<TokenClasses.EnableCut>.Feature
        (TokenClasses.EnableCut tokenClass) => Feature.Extension.Value(EnableCutTokenResult);

    IImplementation ISymbolProviderForPointer<Negate>.Feature(Negate tokenClass)
        => Feature.Extension.Value(NegationResult);

    IImplementation ISymbolProviderForPointer<Operation>.Feature(Operation tokenClass)
        => Feature.Extension.FunctionFeature(OperationResult, tokenClass);

    IImplementation ISymbolProviderForPointer<TextItem>.Feature(TextItem tokenClass)
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

    internal override IEnumerable<IConversion> CutEnabledConversion(NumberType destination)
    {
        yield return
            Feature.Extension.Conversion
                (category => CutEnabledBitCountConversion(category, destination), EnableCut);
    }

    protected override Result ParentConversionResult(Category category)
        => Mutation(Parent) & category;

    protected override Size GetSize() => Parent.Size;

    protected override CodeBase DumpPrintCode() => Align.ArgCode.DumpPrintNumber(Align.Size);

    Result GetZeroResult() => Root
        .BitType
        .Number(1)
        .Result(Category.All, () => CodeBase.BitsConst(BitsConst.Convert(0)));

    Result TextItemResult(Category category) => Parent
        .TextItem
        .Pointer
        .Result
        (
            category,
            Parent
                .Pointer
                .Result(category, ObjectResult(category | Category.Type)));

    Result NegationResult(Category category) => ((NumberType)ZeroResult.Value.Type)
        .OperationResult(category, MinusOperation, this)
        .ReplaceAbsolute
        (
            ZeroResult.Value.Type.ForcedReference,
            c => ZeroResult.Value.LocalReferenceResult & c)
        .ReplaceArg(ObjectResult);

    Result EnableCutTokenResult(Category category)
        => EnableCut
            .Pointer
            .Result(category | Category.Type, ObjectResult(category));

    Result OperationResult(Category category, TypeBase right, IOperation operation)
    {
        var enumerable = ConversionService.FindPathDestination<NumberType>(right);
        if(enumerable == null)
            return null;

        var destination = enumerable.SingleOrDefault();
        if(destination != null)
            return OperationResult(category, operation, destination)
                .ReplaceArg(c => right.Conversion(c, destination.Pointer));

        NotImplementedMethod(category, right, operation);
        return null;
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
        var result = resultType.Result
        (
            category,
            () => OperationCode(resultType.Size, operationName, right),
            Closures.Arg
        );

        var leftResult = ObjectResult(category | Category.Type)
            .Conversion(Align);
        var rightResult = right.Pointer.ArgResult(category | Category.Type).Conversion(right.Align);
        var pair = leftResult + rightResult;
        return result.ReplaceArg(pair);
    }

    CodeBase OperationCode(Size resultSize, string token, TypeBase right)
    {
        (!(right is PointerType)).Assert();
        return Align
            .Pair(right.Align)
            .ArgCode
            .NumberOperation(token, resultSize, Align.Size, right.Align.Size);
    }

    Result FlatConversion(Category category, NumberType source)
    {
        if(Bits == source.Bits)
            return ArgResult(category | Category.Type);

        return Result
        (
            category,
            () => source.ArgCode.BitCast(Size),
            Closures.Arg
        );
    }

    Result CutEnabledBitCountConversion(Category category, NumberType destination)
    {
        if(Bits == destination.Bits)
            return EnableCut.Mutation(this) & category;

        return destination
            .Result
            (
                category,
                () => EnableCut.ArgCode.BitCast(destination.Size),
                Closures.Arg
            );
    }
}