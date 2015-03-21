using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using hw.Scanner;
using Reni.Parser;
using Reni.ReniParser;
using Reni.Validation;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class ElseToken : TokenClass
    {
        public const string TokenId = "else";
        public override string Id => TokenId;

        protected override ReniParser.Syntax Infix(ReniParser.Syntax left, IToken token, ReniParser.Syntax right)
            => left.CreateElseSyntax(right.ToCompiledSyntax);

        protected override ReniParser.Syntax Terminal(IToken token)
            => new CompileSyntaxError(IssueId.UnexpectedUseAsTerminal, token.Characters);
    }
}