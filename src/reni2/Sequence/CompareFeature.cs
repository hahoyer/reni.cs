using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Type;

namespace Reni.Sequence
{
    internal sealed class CompareFeature : FeatureBase
    {
        public CompareFeature(ISequenceOfBitBinaryOperation definable)
            : base(definable) { }

        internal override TypeBase ResultType(int objSize, int argsSize) { return TypeBase.Bit; }
    }
}