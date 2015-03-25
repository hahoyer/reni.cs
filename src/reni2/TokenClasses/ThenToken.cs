using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.ReniParser;
using Reni.Validation;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class ThenToken : InfixToken
    {
        public const string TokenId = "then";
        public override string Id => TokenId;

        protected override Checked<Syntax> Infix(Syntax left, SourcePart token, Syntax right)
        {
            var condition = left.ToCompiledSyntax;
            var result = right.CreateThenSyntax(condition.Value);
            return new Checked<Syntax>(result.Value, condition.Issues.plus<Issue>(result.Issues));
        }
    }
}