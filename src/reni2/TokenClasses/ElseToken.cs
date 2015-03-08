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
    sealed class ElseToken : TokenClass, ITokenClassWithId
    {
        public const string Id = "else";
        string ITokenClassWithId.Id => Id;

        protected override ReniParser.Syntax Infix(ReniParser.Syntax left, Token token, ReniParser.Syntax right)
            => left.CreateElseSyntax(new Syntax(token), right.ToCompiledSyntax);

        protected override ReniParser.Syntax Terminal(Token token)
            => new CompileSyntaxError(IssueId.UnexpectedUseAsTerminal, token);

        internal sealed class Syntax : ReniParser.Syntax
        {
            public Syntax(Token token)
                : base(token)
            { }
        }

    }
}