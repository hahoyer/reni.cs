using HWClassLibrary.Debug;
using Reni.Code;
using Reni.Feature;
using Reni.Parser.TokenClass;

namespace Reni.Type
{
    internal sealed class Bit : Primitive
    {
        [DumpData(false)]
        internal override Size Size { get { return Size.Create(1); } }
        internal override string DumpPrintText { get { return "bit"; } }
        internal override int SequenceCount { get { return 1; } }

        public override bool HasEmptyValue()
        {
            return true;
        }

        internal override Result SequenceDumpPrint(Category category, int count)
        {
            return CreateSequence(count).CreateArgResult(category).DumpPrintBitSequence();
        }

        internal override Result DumpPrint(Category category)
        {
            return CreateArgResult(category).DumpPrintBitSequence();
        }

        internal override bool IsConvertableToVirt(TypeBase dest, ConversionFeature conversionFeature)
        {
            if(conversionFeature.IsUseConverter)
                return dest.HasConverterFromBit();

            return false;
        }

        internal override CodeBase CreateSequenceOperation(Defineable token, Size size, Size objSize, Size argsSize)
        {
            return CreateSequence((objSize.ByteAlignedSize + argsSize.ByteAlignedSize).ToInt())
                .CreateArgCode()
                .CreateBitSequenceOperation(token, size, objSize.ByteAlignedSize);
        }

        internal override CodeBase CreateSequenceOperation(Defineable token, Result result)
        {
            return result.Code.CreateBitSequenceOperation(token);
        }

        internal override TypeBase SequenceOperationResultType(Defineable token, int objBitCount, int argBitCount)
        {
            return token.BitSequenceOperationResultType(objBitCount, argBitCount);
        }

        internal override SearchResult<ISequenceElementFeature> SearchFromSequence(Defineable defineable)
        {
            var result = defineable.SearchFromSequenceOfBit();
            return result.SearchResultDescriptor.Convert(result.Feature);
        }

        internal override SearchResult<ISequenceElementPrefixFeature> SearchPrefixFromSequence(Defineable defineable)
        {
            var result = defineable.SearchPrefixFromSequenceOfBit();
            return result.SearchResultDescriptor.Convert
                (result.Feature);
        }

        public override string Dump()
        {
            return GetType().FullName;
        }

        public override string DumpShort()
        {
            return "bit";
        }
    }
}