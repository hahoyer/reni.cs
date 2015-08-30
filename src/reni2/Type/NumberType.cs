using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Numeric;
using Reni.TokenClasses;

namespace Reni.Type
{
    sealed class NumberType
        : Child<ArrayType>
            , ISymbolProviderForPointer<DumpPrintToken>
            , ISymbolProviderForPointer<Operation>
            , ISymbolProviderForPointer<TokenClasses.EnableCut>
            , ISymbolProviderForPointer<Negate>
            , ISymbolProviderForPointer<TextItem>
            , IForcedConversionProvider<NumberType>
    {
        static readonly Minus _minusOperation = new Minus();
        readonly ValueCache<Result> _zeroResult;

        public NumberType(ArrayType parent)
            : base(parent)
        {
            _zeroResult = new ValueCache<Result>(GetZeroResult);
        }

        Result GetZeroResult()
        {
            return RootContext
                .BitType
                .Number(1)
                .Result(Category.All, () => CodeBase.BitsConst(BitsConst.Convert(0)));
        }

        [DisableDump]
        internal override Root RootContext => Parent.RootContext;
        [DisableDump]
        internal override bool Hllw => Parent.Hllw;
        [DisableDump]
        internal override string DumpPrintText => "number(bits:" + Bits + ")";
        [DisableDump]
        internal override bool IsCuttingPossible => true;
        [EnableDump]
        internal int Bits => Size.ToInt();
        [DisableDump]
        protected override IEnumerable<IGenericProviderForType> Genericize => this.GenericListFromType(base.Genericize);

        internal override IEnumerable<IValue> CutEnabledConversion(NumberType destination)
        {
            yield return Feature.Extension.Value(category => CutEnabledBitCountConversion(category, destination), EnableCut);
        }

        ITypeImplementation ISymbolProviderForPointer<DumpPrintToken>.Feature
            (DumpPrintToken tokenClass)
            => Feature.Extension.Value(DumpPrintTokenResult, this);

        ITypeImplementation ISymbolProviderForPointer<Operation>.Feature(Operation tokenClass)
            => Feature.Extension.FunctionFeature(OperationResult, tokenClass);

        ITypeImplementation ISymbolProviderForPointer<TokenClasses.EnableCut>.Feature
            (TokenClasses.EnableCut tokenClass) => Feature.Extension.Value(EnableCutTokenResult);

        IEnumerable<IValue> IForcedConversionProvider<NumberType>.Result(NumberType destination)
        {
            if(Bits <= destination.Bits)
                yield return Feature.Extension.Value(category => destination.FlatConversion(category, this), this);
        }

        ITypeImplementation ISymbolProviderForPointer<Negate>.Feature(Negate tokenClass)
            => Feature.Extension.Value(NegationResult);

        ITypeImplementation ISymbolProviderForPointer<TextItem>.Feature(TextItem tokenClass)
            => Feature.Extension.Value(TextItemResult);

        protected override Result ParentConversionResult(Category category) => Parent.Result(category, ArgResult);

        protected override Size GetSize() => Parent.Size;

        Result TextItemResult(Category category) => Parent
            .TextItem
            .Pointer
            .Result
            (
                category,
                Parent
                    .Pointer
                    .Result(category, ObjectResult(category.Typed)));

        Result NegationResult(Category category) => ((NumberType) _zeroResult.Value.Type)
            .OperationResult(category, _minusOperation, this)
            .ReplaceAbsolute(_zeroResult.Value.Type.ForcedReference, c => _zeroResult.Value.LocalReferenceResult & (c))
            .ReplaceArg(ObjectResult);

        protected override CodeBase DumpPrintCode() => Align.ArgCode.DumpPrintNumber(Align.Size);

        Result EnableCutTokenResult(Category category)
            => EnableCut
                .Pointer
                .Result(category.Typed, ObjectResult(category));

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
                ? (TypeBase) RootContext.BitType
                : RootContext.BitType.Number(transformation.Signature(Bits, right.Bits));
            return OperationResult(category, resultType, operation.Name, right);
        }

        Result OperationResult(Category category, TypeBase resultType, string operationName, NumberType right)
        {
            var result = resultType.Result
                (
                    category,
                    () => OperationCode(resultType.Size, operationName, right),
                    CodeArgs.Arg
                );

            var leftResult = ObjectResult(category.Typed)
                .Conversion(Align);
            var rightResult = right.Pointer.ArgResult(category.Typed).Conversion(right.Align);
            var pair = leftResult + rightResult;
            return result.ReplaceArg(pair);
        }

        CodeBase OperationCode(Size resultSize, string token, TypeBase right)
        {
            Tracer.Assert(!(right is PointerType));
            return Align
                .Pair(right.Align)
                .ArgCode
                .NumberOperation(token, resultSize, Align.Size, right.Align.Size);
        }

        Result FlatConversion(Category category, NumberType source)
        {
            if(Bits == source.Bits)
                return ArgResult(category.Typed);

            return Result
                (
                    category,
                    () => source.ArgCode.BitCast(Size),
                    CodeArgs.Arg
                );
        }

        Result CutEnabledBitCountConversion(Category category, NumberType destination)
        {
            if(Bits == destination.Bits)
                return Result(category, EnableCut.ArgResult(category));

            return destination
                .Result
                (
                    category,
                    () => EnableCut.ArgCode.BitCast(destination.Size),
                    CodeArgs.Arg
                );
        }

        internal interface IOperation
        {
            [DisableDump]
            string Name { get; }
        }

        internal interface ITransformation
        {
            int Signature(int objectBitCount, int argsBitCount);
        }
    }
}