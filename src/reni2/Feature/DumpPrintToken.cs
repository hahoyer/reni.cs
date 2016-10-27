using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
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
        internal override IEnumerable<IDeclarationProvider> Genericize
            => this.GenericListFromDefinable(base.Genericize);
    }
}