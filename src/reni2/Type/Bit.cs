using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Code;
using Reni.Parser.TokenClass;

namespace Reni.Type
{
    [Serializable]
    internal sealed class Bit : TypeBase
    {
        protected override Size GetSize() { return Size.Create(1); }

        internal override string DumpPrintText { get { return "bit"; } }
        internal override int SequenceCount { get { return 1; } }

        internal override Result SequenceDumpPrint(Category category, int count) { return CreateSequence(count).CreateArgResult(category).DumpPrintBitSequence(); }

        internal new Result DumpPrint(Category category) { return CreateArgResult(category).DumpPrintBitSequence(); }

        internal override bool IsConvertableToImplementation(TypeBase dest, ConversionFeature conversionFeature)
        {
            if(conversionFeature.IsUseConverter)
                return dest.HasConverterFromBit();

            return false;
        }

        protected override CodeBase CreateSequenceOperation(Size size, ISequenceOfBitBinaryOperation token, Size objSize, Size argsSize)
        {
            return CreateSequence((objSize.ByteAlignedSize + argsSize.ByteAlignedSize).ToInt())
                .CreateArgCode()
                .CreateBitSequenceOperation(token, size, objSize.ByteAlignedSize);
        }

        protected override CodeBase CreateSequenceOperation(Size size, ISequenceOfBitPrefixOperation feature, Size objSize)
        {
            return CreateSequence((objSize.ByteAlignedSize).ToInt())
                .CreateArgCode()
                .CreateBitSequenceOperation(feature, size);
        }

        protected override TypeBase SequenceOperationResultType(ISequenceOfBitBinaryOperation token, int objBitCount, int argBitCount)
        {
            return token.ResultType(objBitCount, argBitCount);
        }

        protected override TypeBase SequenceOperationResultType(ISequenceOfBitPrefixOperation token, int objBitCount)
        {
            return token.ResultType(objBitCount);
        }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            searchVisitor.Child(this).Search();
            base.Search(searchVisitor);
        }

        public override string Dump() { return GetType().FullName; }

        internal override string DumpShort() { return "bit"; }
    }

    internal interface ISequenceOfBitPrefixOperation : ISequenceOfBitOperation
    {
        TypeBase ResultType(int objBitCount);
    }

    internal interface ISequenceOfBitBinaryOperation
    {
        TypeBase ResultType(int objBitCount, int argBitCount);
        bool IsCompareOperator { get; }
        string DataFunctionName { get; }
        string CSharpNameOfDefaultOperation { get; }
    }
}