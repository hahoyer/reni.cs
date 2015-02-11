using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Feature;
using Reni.Numeric;
using Reni.ReniParser;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    [Variant(false)]
    [Variant(true)]
    sealed class EqualityOperation : Operation, ITokenClassWithId
    {
        public static string Id(bool isEqual=true) => isEqual ? "=" : "<>";

        public EqualityOperation(bool isEqual) { IsEqual = isEqual; }
        bool IsEqual { get; }
        string ITokenClassWithId.Id => Id(IsEqual);

        [DisableDump]
        internal override IEnumerable<IGenericProviderForDefinable> Genericize => this.GenericListFromDefinable(base.Genericize);
    }
}