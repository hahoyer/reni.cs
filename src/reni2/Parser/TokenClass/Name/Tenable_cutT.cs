using System;
using Reni.Feature;
using Reni.Type;

namespace Reni.Parser.TokenClass.Name
{
    [Token("enable_cut")]
    [Serializable]

    internal sealed class Tenable_cutT : Defineable, IFeatureForSequence
    {
        public IFeature Convert(Sequence type)
        {
            return type.EnableCutFeatureObject();
        }

        internal override SearchResult<IFeatureForSequence> SearchForSequence()
        {
            return SearchResult<IFeatureForSequence>.Success(this, this);
        }
    }
}
