using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.Parser;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class ThenToken : InfixToken
    {
        public const string TokenId = "then";
        public override string Id => TokenId;

        protected override Result<OldSyntax> OldInfix(OldSyntax left, SourcePart token, OldSyntax right)
        {
            var condition = left.ToCompiledSyntax;
            var result = right.CreateThenSyntax(condition.Target);
            return new Result<OldSyntax>(result.Target, condition.Issues.plus(result.Issues));
        }
    }
}