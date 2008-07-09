using HWClassLibrary.Debug;
using Reni.Feature;
using Reni.Type;

namespace Reni.Parser.TokenClass
{
    internal abstract class SequenceOfBitOperation : Defineable,
        IConverter<IConverter<IFeature, Sequence>,Bit>,
        IConverter<IConverter<IPrefixFeature, Sequence>, Bit>,
        IConverter<IFeature, Sequence>,
        IConverter<IPrefixFeature, Sequence>
    {
        [DumpExcept(false)]
        internal protected virtual bool IsBitSequencePrefixOperation { get { return false; } }

        IFeature IConverter<IFeature, Sequence>.Convert(Sequence type)
        {
            return type.BitOperationFeature(this);
        }

        IConverter<IFeature, Sequence> IConverter<IConverter<IFeature, Sequence>,Bit>.Convert(Bit type)
        {
            return this;
        }

        IConverter<IPrefixFeature, Sequence> IConverter<IConverter<IPrefixFeature, Sequence>, Bit>.Convert(Bit type)
        {
            return this;
        }

        IPrefixFeature IConverter<IPrefixFeature, Sequence>.Convert(Sequence type)
        {
            return type.BitOperationPrefixFeature(this);
        }

        internal override sealed SearchResult<IConverter<IConverter<IFeature, Sequence>, Bit>> SearchFromSequenceOfBit()
        {
            return SearchResult<IConverter<IConverter<IFeature, Sequence>, Bit>>.Success(this, this);
        }

        internal override sealed SearchResult<IConverter<IConverter<IPrefixFeature, Sequence>, Bit>> SearchPrefixFromSequenceOfBit()
        {
            if(IsBitSequencePrefixOperation)
                return SearchResult<IConverter<IConverter<IPrefixFeature, Sequence>, Bit>>.Success(this, this);
            return base.SearchPrefixFromSequenceOfBit();
        }
    }
}