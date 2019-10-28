using Bnf.TokenClasses;
using hw.DebugFormatter;
using hw.Parser;

namespace Bnf.Scanner
{
    sealed class TokenFactory : GenericTokenFactory
    {
        public TokenFactory(string title = null)
            : base(title) {}

        protected override hw.Scanner.ITokenType NewSymbol(string name)
        {
            Tracer.ConditionalBreak(name == "identifier");
            return new Declarator(name);
        }
    }
}