using System;
using HWClassLibrary.Debug;
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

        internal override bool IsConvertableToVirt(TypeBase dest, ConversionFeature conversionFeature)
        {
            if(conversionFeature.IsUseConverter)
                return dest.HasConverterFromBit();

            return false;
        }

        internal override CodeBase CreateSequenceOperation(Size size, Defineable token, Size objSize, Size argsSize)
        {
            return CreateSequence((objSize.ByteAlignedSize + argsSize.ByteAlignedSize).ToInt())
                .CreateArgCode()
                .CreateBitSequenceOperation(token, size, objSize.ByteAlignedSize);
        }

        internal override CodeBase CreateSequenceOperation(Defineable token, Result result)
        {
            return result.Code.CreateBitSequenceOperation(token);
        }

        protected internal override TypeBase SequenceOperationResultType(Defineable token, int objBitCount, int argBitCount)
        {
            return token.BitSequenceOperationResultType(objBitCount, argBitCount);
        }

        internal override SearchResult<IConverter<IFeature, Sequence>> SearchFromSequence(Defineable defineable)
        {
            var result = defineable.SearchFromSequenceOfBit();
            return result.SearchResultDescriptor.Convert(result.Feature, this);
        }

        internal override SearchResult<IConverter<IPrefixFeature, Sequence>> SearchPrefixFromSequence(Defineable defineable)
        {
            var result = defineable.SearchPrefixFromSequenceOfBit();
            return result.SearchResultDescriptor.Convert(result.Feature, this);
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