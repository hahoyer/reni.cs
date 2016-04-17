using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Parser;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class ThenToken : InfixToken
    {
        public const string TokenId = "then";
        public override string Id => TokenId;
    }
}