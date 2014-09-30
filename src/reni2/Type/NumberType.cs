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

namespace Reni.Type
{
    sealed class NumberType
        : TypeBase
            , ISymbolProvider<DumpPrintToken, IFeatureImplementation>, ISymbolProvider<Operation, IFeatureImplementation>,
            ISymbolProvider<TokenClasses.EnableCut, IFeatureImplementation>,
            IConverterProvider<NumberType, IFeatureImplementation>
    {
        readonly FunctionCache<RefAlignParam, ObjectReference> _objectReferencesCache;
        readonly ArrayType _parent;

        public NumberType(ArrayType parent)
        {
            _parent = parent;
            _objectReferencesCache = new FunctionCache<RefAlignParam, ObjectReference>
                (refAlignParam => new ObjectReference(this, refAlignParam));
        }

        [DisableDump]
        internal override Root RootContext { get { return _parent.RootContext; } }
        protected override Size GetSize() { return _parent.Size; }
        [DisableDump]
        internal override bool Hllw { get { return _parent.Hllw; } }
        [DisableDump]
        internal override string DumpPrintText { get { return "number(" + Bits + ")"; } }
        [EnableDump]
        internal int Bits { get { return Size.ToInt(); } }
        [DisableDump]
        protected override IEnumerable<IGenericProviderForType> Genericize
        {
            get { return this.GenericListFromType(base.Genericize); }
        }

        internal ObjectReference UniqueObjectReference(RefAlignParam refAlignParam)
        {
            return _objectReferencesCache[refAlignParam];
        }

        IFeatureImplementation ISymbolProvider<DumpPrintToken, IFeatureImplementation>.Feature(DumpPrintToken token)
        {
            return Extension.SimpleFeature(DumpPrintTokenResult);
        }

        IFeatureImplementation ISymbolProvider<Operation, IFeatureImplementation>.Feature(Operation token)
        {
            return Extension.FunctionFeature(OperationResult, token);
        }

        IFeatureImplementation ISymbolProvider<TokenClasses.EnableCut, IFeatureImplementation>.Feature
            (TokenClasses.EnableCut token)
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

        Result DumpPrintTokenResult(Category category) { return VoidType.Result(category, DumpPrintNumberCode, CodeArgs.Arg); }
        Result EnableCutTokenResult(Category category) { return UniqueEnableCutType.UniquePointer.ArgResult(category.Typed); }

        Result OperationResult(Category category, IContextReference left, TypeBase right, IOperation operation)
        {
            var rightNumber = right.SmartUn<PointerType>() as NumberType;
            if(rightNumber != null)
                return OperationResult(category, left, rightNumber, operation);

            NotImplementedMethod(category, left, right, operation);
            return null;
        }

        Result OperationResult(Category category, IContextReference left, NumberType right, IOperation operation)
        {
            var leftBits = Bits;
            var rightBits = right.Bits;
            var resultBits = operation.Signature(leftBits, rightBits);
            var resultType = RootContext.BitType.UniqueNumber(resultBits);
            var result = resultType.Result
                (
                    category,
                    () => ApplyCode(resultType.Size, operation.Name, right),
                    CodeArgs.Arg
                );
            var leftResult = UniqueObjectReference(Root.DefaultRefAlignParam).Result(category.Typed);
            return result.ReplaceArg(leftResult + right.UniquePointer.ArgResult(category.Typed));
        }

        CodeBase ApplyCode(Size size, string token, TypeBase right)
        {
            return UniquePointer.Pair(right.UniquePointer).ArgCode
                .NumberOperation(token, size, Size.ByteAlignedSize);
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

        internal interface IOperation
        {
            int Signature(int objectBitCount, int argsBitCount);

            [DisableDump]
            string Name { get; }
        }
    }
}