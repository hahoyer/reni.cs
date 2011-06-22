using System;
using System.Linq;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using Reni.Context;
using Reni.Feature;
using Reni.Sequence;

namespace Reni.Type
{
    [Serializable]
    internal class EnableCutFeature : ReniObject, IFeature
    {
        private readonly BaseType _sequence;
        public EnableCutFeature(BaseType sequence) { _sequence = sequence; }
        TypeBase IFeature.DefiningType() { return _sequence; }
        Result IFeature.Apply(Category category, RefAlignParam refAlignParam) { return new EnableCut(_sequence).ArgResult(category); }
    }
}