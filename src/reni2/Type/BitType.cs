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
    sealed class BitType
        : TypeBase
            , ISymbolProvider<DumpPrintToken, IPath<IPath<IFeatureImplementation, SequenceType>, ArrayType>>
    {
        readonly Root _rootContext;

        internal BitType(Root rootContext) { _rootContext = rootContext; }

        [DisableDump]
        internal override bool Hllw { get { return false; } }

        [DisableDump]
        internal override string DumpPrintText { get { return "bit"; } }

        [DisableDump]
        internal override Root RootContext { get { return _rootContext; } }

        protected override Size GetSize() { return Size.Create(1); }

        IPath<IPath<IFeatureImplementation, SequenceType>, ArrayType>
            ISymbolProvider<DumpPrintToken, IPath<IPath<IFeatureImplementation, SequenceType>, ArrayType>>.Feature
        {
            get
            {
                var feature = Extension
                    .Feature<SequenceType, ArrayType>(DumpPrintTokenResult);
                return feature;
            }
        }

        Result DumpPrintTokenResult(Category category, SequenceType sequenceType, ArrayType arrayType)
        {
            Tracer.Assert(sequenceType.Parent == arrayType);
            Tracer.Assert(arrayType.ElementType == this);
            return VoidType
                .Result(category, sequenceType.DumpPrintNumberCode, CodeArgs.Arg);
        }

        internal Result DumpPrintTokenResult(Category category)
        {
            return VoidType
                .Result(category, DumpPrintNumberCode, CodeArgs.Arg);
        }

        protected override string Dump(bool isRecursion) { return GetType().PrettyName(); }

        protected override string GetNodeDump() { return "bit"; }
        internal NumberType UniqueNumber(int bitCount) { return UniqueArray(bitCount).UniqueNumber; }
        internal Result Result(Category category, BitsConst bitsConst)
        {
            return UniqueNumber(bitsConst.Size.ToInt())
                .Result(category, () => CodeBase.BitsConst(bitsConst));
        }
        internal CodeBase ApplyCode(Size size, string token, int objectBits, int argsBits)
        {
            var objectType = UniqueNumber(objectBits).UniqueAlign;
            var argsType = UniqueNumber(argsBits).UniqueAlign;
            return objectType
                .Pair(argsType).ArgCode
                .BitSequenceOperation(token, size, Size.Create(objectBits).ByteAlignedSize);
        }

        internal interface IOperation
        {
            int Signature(int objectBitCount, int argsBitCount);

            [DisableDump]
            string Name { get; }
        }

        internal interface IPrefix
        {
            [DisableDump]
            string Name { get; }
        }

        Result ApplyResult(Category category, IOperation operation, int objectBitCount, TypeBase argsType)
        {
            var typedCategory = category.Typed;
            var argsBitCount = argsType.SequenceLength(this);
            var resultBitCount = operation.Signature(objectBitCount, argsBitCount);
            var result = UniqueNumber(resultBitCount)
                .Result
                (
                    category,
                    () => ApplyCode(Size.Create(resultBitCount), operation.Name, objectBitCount, argsBitCount),
                    CodeArgs.Arg);
            var objectResult = UniqueNumber(objectBitCount).UniqueObjectReference(Root.DefaultRefAlignParam).Result(typedCategory);
            var convertedObjectResult = objectResult.BitSequenceOperandConversion(typedCategory);
            var convertedArgsResult = argsType.BitSequenceOperandConversion(typedCategory);
            return result.ReplaceArg(convertedObjectResult + convertedArgsResult);
        }

        Result PrefixResult(Category category, string operation, int objectBitCount)
        {
            var objectType = UniqueNumber(objectBitCount);
            return objectType
                .Result(category, () => objectType.BitSequenceOperation(operation), CodeArgs.Arg)
                .ReplaceArg
                (
                    category1
                        => objectType
                            .UniquePointer
                            .ArgResult(category1.Typed).AutomaticDereferenceResult
                            .Align(Root.DefaultRefAlignParam.AlignBits));
        }
    }
}