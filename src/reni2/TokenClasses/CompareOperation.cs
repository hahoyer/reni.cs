using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using Reni.Feature;
using Reni.Numeric;
using Reni.Parser;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    [Variant(false, false)]
    [Variant(false, true)]
    [Variant(true, false)]
    [Variant(true, true)]
    sealed class CompareOperation : Operation
    {
        public static string TokenId(bool isLess = true, bool canBeEqual = false)
            => (isLess ? "<" : ">") + (canBeEqual ? "=" : "");

        public CompareOperation(bool isLess, bool canBeEqual)
        {
            IsLess = isLess;
            CanBeEqual = canBeEqual;
        }

        bool IsLess { get; }
        bool CanBeEqual { get; }
        public override string Id => TokenId(IsLess, CanBeEqual);

        [DisableDump]
        internal override IEnumerable<IDeclarationProvider> MakeGeneric
            => this.GenericListFromDefinable(base.MakeGeneric);
    }
}