using System;
using System.Linq;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using Reni.Feature;

namespace Reni.Type
{
    [Serializable]
    internal class SequenceOperationPrefixFeature : ReniObject, ISearchPath<IPrefixFeature, Sequence>
    {
        [IsDumpEnabled(true)]
        private readonly ISequenceOfBitPrefixOperation _definable;

        private readonly Bit _bit;

        public SequenceOperationPrefixFeature(Bit bit, ISequenceOfBitPrefixOperation definable)
        {
            _bit = bit;
            _definable = definable;
        }

        IPrefixFeature ISearchPath<IPrefixFeature, Sequence>.Convert(Sequence type) { return type.PrefixFeature(_definable); }
    }
}