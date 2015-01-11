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
using Reni.Numeric;
using Reni.TokenClasses;
using Reni.Type.ConversionService;

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

        internal override IEnumerable<ISimpleFeature> CutEnabledConversion(NumberType destination)
        {
            yield return Extension.SimpleFeature(category => BitCountConversion(category, destination), EnableCut);
        }

        IFeatureImplementation ISymbolProvider<DumpPrintToken, IFeatureImplementation>.Feature(DumpPrintToken tokenClass)
        {
            return Extension.SimpleFeature(DumpPrintTokenResult, this);
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

        IEnumerable<ISimpleFeature> IForcedConversionProvider<NumberType>.Result(NumberType destination)
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
                .ReplaceArg(c => PointerConversionResult(c, Parent));
        }

        Result NegationResult(Category category)
        {
            return ((NumberType) _zeroResult.Value.Type)
                .OperationResult(category, _minusOperation, this)
                .ReplaceAbsolute(_zeroResult.Value.Type.Reference, c => _zeroResult.Value.LocalPointerKindResult & (c));
        }

        protected override CodeBase DumpPrintCode()
        {
            var alignedSize = Size.Align(Root.DefaultRefAlignParam.AlignBits);
            return Pointer
                .ArgCode
                .DePointer(alignedSize)
                .DumpPrintNumber(alignedSize);
        }

        Result EnableCutTokenResult(Category category)
        {
            return EnableCut
                .Pointer
                .Result(category.Typed, Pointer.ArgResult(category));
        }

        Result OperationResult(Category category, TypeBase right, IOperation operation)
        {
            var path = FindPath(right, x => x is NumberType);
            if(path != null)
                return OperationResult(category, operation, (NumberType) path.Destination);

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

            var leftResult = Pointer.Result(category.Typed, Reference)
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

        Result BitCountConversion(Category category, NumberType destination)
        {
            if(Bits == destination.Bits)
                return ArgResult(category.Typed);

            return destination
                .Result
                (
                    category,
                    () => ArgCode.BitCast(destination.Size),
                    CodeArgs.Arg
                );
        }

        Result UnalignedDereferencePointerResult(Category category)
        {
            return SmartPointer.ArgResult(category.Typed).DereferenceResult & category;
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