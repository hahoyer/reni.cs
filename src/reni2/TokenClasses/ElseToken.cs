using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using hw.Scanner;
using Reni.ReniParser;
using Reni.Validation;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class ElseToken : TokenClass, ITokenClassWithId
    {
        public const string Id = "else";
        string ITokenClassWithId.Id => Id;
        protected override Syntax Infix(Syntax left, Token token, Syntax right)
            => left.CreateElseSyntax(token, right.ToCompiledSyntax);
        protected override Syntax Terminal(Token token)
            => new CompileSyntaxError(IssueId.UnexpectedUseAsTerminal, token, null);
    }
}