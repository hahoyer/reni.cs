using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using Reni.Feature;
using Reni.Parser;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class TextItem : Definable
    {
        [DisableDump]
        internal override IEnumerable<IDeclarationProvider> Genericize
            => this.GenericListFromDefinable(base.Genericize);
        public const string TokenId = "text_item";
        public override string Id => TokenId;
    }
}