using System.Collections.Generic;
using System.Linq;
using System;
using hw.Debug;
using Reni.Feature;
using Reni.ReniParser;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class TextItem : Definable, ITokenClassWithId
    {
        [DisableDump]
        internal override IEnumerable<IGenericProviderForDefinable> Genericize => this.GenericListFromDefinable(base.Genericize);
        public const string Id = "text_item";
        string ITokenClassWithId.Id => Id;
    }
}