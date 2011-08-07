using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Code;
using Reni.Type;

namespace Reni.Sequence
{
    internal sealed class Feature : FeatureBase
    {
        public Feature(ISequenceOfBitBinaryOperation definable)
            : base(definable) { }

        internal override TypeBase ResultType(int objSize, int argsSize) { return TypeBase.UniqueNumber(Definable.ResultSize(objSize, argsSize)); }
    }
}