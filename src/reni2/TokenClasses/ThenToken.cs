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

        protected override ReniParser.Syntax Infix(ReniParser.Syntax left, IToken token, ReniParser.Syntax right)
            => right.CreateThenSyntax(new Syntax(token), left.ToCompiledSyntax);

        protected override ReniParser.Syntax Terminal(IToken token)
            => new CompileSyntaxError(IssueId.UnexpectedUseAsTerminal, token);

        internal sealed class Syntax : ReniParser.Syntax
        {
            public Syntax(IToken token)
                : base(token)
            { }

            internal override bool IsBraceLike => true;
        }
    }
}