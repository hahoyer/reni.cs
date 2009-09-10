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

        bool IFeature.IsEval { get { return true; } }
        TypeBase IFeature.ResultType { get { return null; } }

        Result IFeature.Apply(Category category, Result objectResult)
        {
            return objectResult.Type.ConvertTo(category, new EnableCut(_sequence))
                .UseWithArg(objectResult);
        }
    }
}