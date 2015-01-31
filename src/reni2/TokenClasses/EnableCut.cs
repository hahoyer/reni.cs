using System.Collections.Generic;
using System.Linq;
using System;
using hw.Debug;
using Reni.Feature;

namespace Reni.TokenClasses
{
    sealed class EnableCut : Definable
    {
        [DisableDump]
        internal override IEnumerable<IGenericProviderForDefinable> Genericize => this.GenericListFromDefinable(base.Genericize);
    }
}