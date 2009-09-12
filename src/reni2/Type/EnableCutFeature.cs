using System;
using System.Linq;
using System.Collections.Generic;
using Reni.Feature;

namespace Reni.Type
{
    [Serializable]
    internal class EnableCutFeature : ReniObject, IFeature
    {
        private readonly Sequence _sequence;

        public EnableCutFeature(Sequence sequence) { _sequence = sequence; }

        TypeBase IFeature.ResultType { get { return null; } }

        Result IFeature.Apply(Category category, TypeBase objectType)
        {
            return objectType.ConvertTo(category, new EnableCut(_sequence));
        }
    }
}