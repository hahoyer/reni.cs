using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Feature;
using Reni.Numeric;

namespace Reni.TokenClasses
{
    sealed class CompareOperation : Operation
    {
        [DisableDump]
        internal override IEnumerable<IGenericProviderForDefinable> Genericize => this.GenericListFromDefinable(base.Genericize);
    }
}