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
    sealed class EqualityOperation : Operation
    {
        public static string TokenId(bool isEqual = true) => isEqual ? "=" : "<>";

        public EqualityOperation(bool isEqual) { IsEqual = isEqual; }
        bool IsEqual { get; }
        public override string Id => TokenId(IsEqual);

        [DisableDump]
        internal override IEnumerable<IGenericProviderForDefinable> Genericize
            => this.GenericListFromDefinable(base.Genericize);
    }
}