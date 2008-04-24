using Reni.Context;
using Reni.Feature;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Parser.TokenClass.Name
{
    internal sealed class Tenable_cutT : Defineable, ISequenceFeature
    {
        internal override SearchResult<ISequenceFeature> SearchFromSequence()
        {
            return SearchResult<ISequenceFeature>.Success(this, this);
        }

        public IFeature Convert(Sequence sequence)
        {
            return sequence.EnableCutFeature();
        }

    }
}