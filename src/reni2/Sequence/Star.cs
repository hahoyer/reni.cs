using System.Collections.Generic;
using System.Linq;
using System;
using hw.Debug;
using Reni.Basics;
using Reni.Feature;

namespace Reni.Sequence
{
    sealed class Star
        : Operation
    {
        protected override int Signature(int objSize, int argSize) { return BitsConst.MultiplySize(objSize, argSize); }
        [DisableDump]
        internal override IEnumerable<IGenericProviderForDefinable> Genericize
        {
            get { return this.GenericListFromDefinable(base.Genericize); }
        }
    }
}