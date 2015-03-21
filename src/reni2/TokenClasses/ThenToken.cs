using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.ReniParser;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class ThenToken : InfixToken
    {
        public const string TokenId = "then";
        public override string Id => TokenId;

        protected override Syntax Infix(Syntax left, SourcePart token, Syntax right)
            => right.CreateThenSyntax(left.ToCompiledSyntax);
    }
}