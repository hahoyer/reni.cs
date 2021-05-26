using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Parser;
using Reni.Feature;
using Reni.Parser;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class EnableCut : Definable
    {
        public const string TokenId = "enable_cut";
        public override string Id => TokenId;
        [DisableDump]
        internal override IEnumerable<IDeclarationProvider> MakeGeneric => this.GenericListFromDefinable(base.MakeGeneric);
    }
}