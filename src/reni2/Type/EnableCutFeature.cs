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
        TypeBase IFeature.DefiningType() { return _sequence; }
        Result IFeature.Apply(Category category) { return new EnableCut(_sequence).ArgResult(category); }
    }
}