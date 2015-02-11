using System.Collections.Generic;
using System.Linq;
using System;
using hw.Debug;
using Reni.Feature;
using Reni.ReniParser;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class EnableCut : Definable, ITokenClassWithId
    {
        public const string Id = "enable_cut";
        string ITokenClassWithId.Id => Id;
        [DisableDump]
        internal override IEnumerable<IGenericProviderForDefinable> Genericize => this.GenericListFromDefinable(base.Genericize);
    }
}