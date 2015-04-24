using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.Parser;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class ThenToken : InfixToken, IBelongingsMatcher
    {
        public const string TokenId = "then";
        public override string Id => TokenId;

        protected override Checked<Syntax> Infix(Syntax left, SourcePart token, Syntax right)
        {
            var condition = left.ToCompiledSyntax;
            var result = right.CreateThenSyntax(condition.Value);
            return new Checked<Syntax>(result.Value, condition.Issues.plus(result.Issues));
        }

        bool IBelongingsMatcher.IsBelongingTo(IBelongingsMatcher otherMatcher)
            => otherMatcher is ElseToken;
    }
}