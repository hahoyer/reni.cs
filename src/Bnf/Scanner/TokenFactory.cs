using Bnf.TokenClasses;
using hw.Helper;
using hw.Parser;
using hw.Scanner;

namespace Bnf.Scanner
{
    sealed class TokenFactory : GenericTokenFactory
    {
        public TokenFactory(string title = null)
            : base(title) {}

        protected override hw.Scanner.ITokenType NewSymbol(string name) => new UserSymbol(name);
    }
}