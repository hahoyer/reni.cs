using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Code;
using Reni.Feature;
using Reni.Parser.TokenClass;

namespace Reni.Type
{
    [Serializable]
    internal sealed class Bit : TypeBase
    {
        protected override Size GetSize()
        {
            return Size.Create(1);
        }

        internal override string DumpPrintText { get { return "bit"; } }
        internal override int SequenceCount { get { return 1; } }

        internal override Result SequenceDumpPrint(Category category, int count)
        {
            return CreateSequence(count).CreateArgResult(category).DumpPrintBitSequence();
        }

        internal override Result DumpPrint(Category category)
        {
            return CreateArgResult(category).DumpPrintBitSequence();
        }

        internal override bool IsConvertableToImplementation(TypeBase dest, ConversionFeature conversionFeature)
        {
            if(conversionFeature.IsUseConverter)
                return dest.HasConverterFromBit();

            return false;
        }

        protected override CodeBase CreateSequenceOperation(Size size, Defineable token, Size objSize, Size argsSize)
        {
            return CreateSequence((objSize.ByteAlignedSize + argsSize.ByteAlignedSize).ToInt())
                .CreateArgCode()
                .CreateBitSequenceOperation(token, size, objSize.ByteAlignedSize);
        }

        protected override CodeBase CreateSequenceOperation(Size size, Defineable token, Size objSize)
        {
            return CreateSequence((objSize.ByteAlignedSize).ToInt())
                .CreateArgCode()
                .CreateBitSequenceOperation(token, size);
        }

        protected override TypeBase SequenceOperationResultType(Defineable token, int objBitCount,
                                                                int argBitCount)
        {
            return token.BitSequenceOperationResultType(objBitCount, argBitCount);
        }

        protected override TypeBase SequenceOperationResultType(Defineable token, int objBitCount)
        {
            return token.BitSequenceOperationResultType(objBitCount);
        }

        public override string Dump()
        {
            return GetType().FullName;
        }

        internal override string DumpShort()
        {
            return "bit";
        }
    }
}