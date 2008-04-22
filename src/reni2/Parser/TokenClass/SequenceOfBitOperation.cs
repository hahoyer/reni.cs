using HWClassLibrary.Debug;
using Reni.Feature;
using Reni.Type;

namespace Reni.Parser.TokenClass
{
    internal abstract class SequenceOfBitOperation : Defineable, ISequenceOfBitFeature, ISequenceOfBitPrefixFeature,
        ISequenceFeature, ISequencePrefixFeature
    {
        [DumpExcept(false)]
        internal protected virtual bool IsBitSequencePrefixOperation { get { return false; } }

        IFeature ISequenceFeature.Convert(Sequence sequence)
        {
            return sequence.BitOperationFeature(this);
        }

        ISequenceFeature ISequenceOfBitFeature.Convert()
        {
            return this;
        }

        ISequencePrefixFeature ISequenceOfBitPrefixFeature.Convert()
        {
            return this;
        }

        IPrefixFeature ISequencePrefixFeature.Convert(Sequence sequence)
        {
            return sequence.BitOperationPrefixFeature(this);
        }

        internal override sealed SearchResult<ISequenceOfBitFeature> SearchFromSequenceOfBit()
        {
            return SearchResult<ISequenceOfBitFeature>.Success(this, this);
        }

        internal override sealed SearchResult<ISequenceOfBitPrefixFeature> SearchPrefixFromSequenceOfBit()
        {
            if(IsBitSequencePrefixOperation)
                return SearchResult<ISequenceOfBitPrefixFeature>.Success(this, this);
            return base.SearchPrefixFromSequenceOfBit();
        }
    }
}