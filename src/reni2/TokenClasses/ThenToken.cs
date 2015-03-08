using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using Reni.ReniParser;
using Reni.Validation;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class ThenToken : TokenClass, ITokenClassWithId
    {
        public const string Id = "then";
        string ITokenClassWithId.Id => Id;

        protected override ReniParser.Syntax Infix(ReniParser.Syntax left, Token token, ReniParser.Syntax right)
            => right.CreateThenSyntax(new Syntax(token), left.ToCompiledSyntax);

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