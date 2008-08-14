using System;
using Reni.Feature;
using Reni.Type;

namespace Reni.Parser.TokenClass.Name
{
    [Token("enable_cut")]
    [Serializable]

    internal sealed class Tenable_cutT : Defineable, IConverter<IFeature, Sequence>
    {
        public IFeature Convert(Sequence type)
        {
            return type.EnableCutFeatureObject();
        }

        internal override SearchResult<IConverter<IFeature, Sequence>> SearchForSequence()
        {
            return SearchResult<IConverter<IFeature, Sequence>>.Success(this, this);
        }
    }
}
