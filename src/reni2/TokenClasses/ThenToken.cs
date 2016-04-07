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

        protected override Checked<OldSyntax> OldInfix(OldSyntax left, SourcePart token, OldSyntax right)
        {
            var condition = left.ToCompiledSyntax;
            var result = right.CreateThenSyntax(condition.Value);
            return new Checked<OldSyntax>(result.Value, condition.Issues.plus(result.Issues));
        }
    }
}