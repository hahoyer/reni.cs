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
            => left.CreateElseSyntax(new Syntax(token), right.ToCompiledSyntax);

        protected override ReniParser.Syntax Terminal(IToken token)
            => new CompileSyntaxError(IssueId.UnexpectedUseAsTerminal);

        internal sealed class Syntax : ReniParser.Syntax
        {
            public Syntax(IToken token)
                : base()
            { }

            internal override bool IsBraceLike => true;
        }

    }
}