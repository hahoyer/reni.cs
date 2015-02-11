using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.ReniParser;
using Reni.TokenClasses;

namespace Reni.Feature
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class DumpPrintToken : Definable, ITokenClassWithId
    {
        public const string Id = "dump_print";
        string ITokenClassWithId.Id => Id;
        [DisableDump]
        internal override IEnumerable<IGenericProviderForDefinable> Genericize => this.GenericListFromDefinable(base.Genericize);
    }
}                                                 