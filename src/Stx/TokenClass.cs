using hw.Parser;

namespace Stx
{
    abstract class TokenClass : ParserTokenType<Syntax>
    {
        protected sealed override Syntax Create(Syntax left, IToken token, Syntax right)
            => Syntax.CreateSourceSyntax(left, this, token, right);
    }
}