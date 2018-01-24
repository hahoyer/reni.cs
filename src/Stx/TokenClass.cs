using hw.Parser;

namespace Stx {
    abstract class TokenClass : CommonTokenType<Syntax>
    {
        protected override Syntax Create(Syntax left, IToken token, Syntax right)
            => Syntax.CreateSourceSyntax(left, this, token, right);
    }
}