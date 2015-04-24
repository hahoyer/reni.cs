using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Parser;
using Reni.TokenClasses;

namespace Reni.Feature
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class DumpPrintToken : Definable
    {
        public const string TokenId = "dump_print";
        public override string Id => TokenId;
        [DisableDump]
        internal override IEnumerable<IGenericProviderForDefinable> Genericize => this.GenericListFromDefinable(base.Genericize);
    }
}                                                 