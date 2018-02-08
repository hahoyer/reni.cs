using System;
using Bnf.TokenClasses;
using hw.Parser;

namespace Bnf.Scanner
{
    sealed class TokenFactory : GenericTokenFactory<Syntax>
    {
        public TokenFactory(Func<string, IParserTokenType<Syntax>> newSymbol, string title = null)
            : base(newSymbol, title) {}

        protected override IParserTokenType<Syntax> NewSymbol(string name) => new UserSymbol(name);
    }
}