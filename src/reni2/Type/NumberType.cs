using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Feature.DumpPrint;
using Reni.Sequence;
using Reni.TokenClasses;

namespace Reni.Type
{
    sealed class NumberType
        : Child<ArrayType>
            , ISymbolProvider<DumpPrintToken, IFeatureImplementation>
            , ISymbolProvider<Operation, IFeatureImplementation>
            , ISymbolProvider<TokenClasses.EnableCut, IFeatureImplementation>
            , ISymbolProvider<Negate, IFeatureImplementation>
            , ISymbolProvider<TextItem, IFeatureImplementation>
            , IConverterProvider<NumberType, IFeatureImplementation>
            , ISpecificConversionProvider<NumberType>
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
                .UniqueNumber(1)
                .Result(Category.All, () => CodeBase.BitsConst(BitsConst.Convert(0)));
        }

        [DisableDump]
        internal override Root RootContext { get { return Parent.RootContext; } }
        [DisableDump]
        internal override bool Hllw { get { return Parent.Hllw; } }
        [DisableDump]
        internal override string DumpPrintText { get { return "number(bits:" + Bits + ")"; } }
        [DisableDump]
        internal override bool IsCuttingPossible { get { return true; } }
        [EnableDump]
        internal int Bits { get { return Size.ToInt(); } }
        [DisableDump]
        protected override IEnumerable<IGenericProviderForType> Genericize
        {
            get { return this.GenericListFromType(base.Genericize); }
        }

        IFeatureImplementation ISymbolProvider<DumpPrintToken, IFeatureImplementation>.Feature(DumpPrintToken tokenClass)
        {
            return Extension.SimpleFeature(DumpPrintTokenResult);
        }

        IFeatureImplementation ISymbolProvider<Operation, IFeatureImplementation>.Feature(Operation tokenClass)
        {
            return Extension.FunctionFeature(OperationResult, tokenClass);
        }

        IFeatureImplementation ISymbolProvider<TokenClasses.EnableCut, IFeatureImplementation>.Feature
            (TokenClasses.EnableCut tokenClass)
        {
            return Extension.SimpleFeature(EnableCutTokenResult);
        }

        IFeatureImplementation IConverterProvider<NumberType, IFeatureImplementation>.Feature
            (NumberType destination, IConversionParameter parameter)
        {
            if(!parameter.EnableCut && Bits > destination.Bits)
                return null;
            return Extension.SimpleFeature(ca => ConversionAsReference(ca, destination));
        }

        IEnumerable<ISimpleFeature> ISpecificConversionProvider<NumberType>.Result(NumberType destination)
        {
            if(Bits <= destination.Bits)
                yield return Extension.SimpleFeature(category => destination.FlatConversion(category, this), this);
        }

        IFeatureImplementation ISymbolProvider<Negate, IFeatureImplementation>.Feature(Negate tokenClass)
        {
            return Extension.SimpleFeature(NegationResult);
        }

        IFeatureImplementation ISymbolProvider<TextItem, IFeatureImplementation>.Feature(TextItem tokenClass)
        {
            return Extension.SimpleFeature(TextItemResult);
        }

        protected override Result ParentConversionResult(Category category) { return Parent.Result(category, ArgResult); }

        protected override Size GetSize() { return Parent.Size; }

        Result TextItemResult(Category category)
        {
            return Parent
                .TextItemResult(category)
                .ReplaceArg(ParentConversionResult);
        }

        Result NegationResult(Category category)
        {
            return ((NumberType) _zeroResult.Value.Type)
                .OperationResult(category, this, _minusOperation)
                .ReplaceAbsolute(_zeroResult.Value.Type.UniquePointerType, c => _zeroResult.Value.LocalPointerKindResult & (c));
        }

        Result DumpPrintTokenResult(Category category) { return VoidType.Result(category, DumpPrintNumberCode, CodeArgs.Arg); }
        Result EnableCutTokenResult(Category category) { return UniqueEnableCutType.UniquePointer.ArgResult(category.Typed); }

        Result OperationResult(Category category, TypeBase right, IOperation operation)
        {
            var rightNumber = right.SmartUn<PointerType>().SmartUn<AlignType>() as NumberType;
            if(rightNumber != null)
                return OperationResult(category, rightNumber, operation);

            NotImplementedMethod(category, right, operation);
            return null;
        }

        Result OperationResult(Category category, NumberType right, IOperation operation)
        {
            var leftBits = Bits;
            var rightBits = right.Bits;
            var resultBits = operation.Signature(leftBits, rightBits);
            var resultType = RootContext.BitType.UniqueNumber(resultBits);
            var result = resultType.Result
                (
                    category,
                    () => OperationCode(resultType.Size, operation.Name, right),
                    CodeArgs.Arg
                );

            var leftResult = UniquePointer.Result(category.Typed, UniquePointerType)
                .Conversion(UniqueAlign);
            var rightResult = right.UniquePointer
                .ArgResult(category.Typed)
                .Conversion(right.UniqueAlign);
            var pair = leftResult + rightResult;
            return result.ReplaceArg(pair);
        }

        CodeBase OperationCode(Size resultSize, string token, TypeBase right)
        {
            Tracer.Assert(!(right is PointerType));
            return UniqueAlign.Pair(right.UniqueAlign).ArgCode
                .NumberOperation(token, resultSize, UniqueAlign.Size, right.UniqueAlign.Size);
        }

        Result ConversionAsReference(Category category, NumberType destination)
        {
            return destination
                .FlatConversion(category, this)
                .ReplaceArg(UnalignedDereferencePointerResult)
                .LocalPointerKindResult;
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

        Result UnalignedDereferencePointerResult(Category category)
        {
            return PointerKind.ArgResult(category.Typed).DereferenceResult & category;
        }

        IEnumerable<ISimpleFeature> GetSpecificReverseConversions(NumberType source)
        {
            if(source.Bits <= Bits)
                yield return Extension.SimpleFeature(category => FlatConversion(category, source));
        }

        internal interface IOperation
        {
            int Signature(int objectBitCount, int argsBitCount);

            [DisableDump]
            string Name { get; }
        }
    }
}