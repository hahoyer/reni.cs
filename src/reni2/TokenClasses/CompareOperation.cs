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
    [Variant(false, false)]
    [Variant(false, true)]
    [Variant(true, false)]
    [Variant(true, true)]
    sealed class CompareOperation : Operation
    {
        public static string TokenId(bool isLess=true, bool canBeEqual=false) => (isLess ? "<" : ">") + (canBeEqual ? "=" : "");

        public CompareOperation(bool isLess, bool canBeEqual)
        {
            IsLess = isLess;
            CanBeEqual = canBeEqual;
        }

        bool IsLess { get; }
        bool CanBeEqual { get; }
        public override string Id => TokenId(IsLess, CanBeEqual);

        [DisableDump]
        internal override IEnumerable<IGenericProviderForDefinable> Genericize => this.GenericListFromDefinable(base.Genericize);
    }
}