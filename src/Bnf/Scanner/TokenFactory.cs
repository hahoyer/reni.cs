using Bnf.TokenClasses;
using hw.Parser;

namespace Bnf.Scanner
{
    sealed class TokenFactory : GenericTokenFactory
    {
        public TokenFactory(string title = null)
            : base(title) {}

        protected override hw.Scanner.ITokenType NewSymbol(string name) => new Declarator(name);
    }
}