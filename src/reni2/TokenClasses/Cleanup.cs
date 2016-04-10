using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Parser;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class Cleanup : TokenClass
    {
        public const string TokenId = "~~~";
        public override string Id => TokenId;
    }
}