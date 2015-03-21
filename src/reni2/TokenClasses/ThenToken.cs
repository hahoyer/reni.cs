using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using Reni.ReniParser;
using Reni.Validation;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class ThenToken : TokenClass
    {
        public const string TokenId = "then";
        public override string Id => TokenId;

        protected override Syntax Infix
            (Syntax left, IToken token, Syntax right)
            => right.CreateThenSyntax(left.ToCompiledSyntax);

        protected override Syntax Terminal(IToken token)
            => new CompileSyntaxError(IssueId.UnexpectedUseAsTerminal, token.Characters);
    }
}