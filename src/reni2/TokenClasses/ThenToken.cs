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
        protected override Syntax Infix(Syntax left, Token token, Syntax right)
            => right.CreateThenSyntax(token, left.ToCompiledSyntax);
        protected override Syntax Terminal(Token token)
            => new CompileSyntaxError(IssueId.UnexpectedUseAsTerminal, token, null);
    }
}