using System.Linq;
using System.Collections.Generic;
using System;
using hw.Debug;
using Reni.Feature;

namespace Reni.TokenClasses
{
    sealed class EnableArrayOverSize : Definable
    {
        internal static readonly string Id = "enable_array_oversize";
        [DisableDump]
        internal override IEnumerable<IGenericProviderForDefinable> Genericize => this.GenericListFromDefinable(base.Genericize);
    }
}