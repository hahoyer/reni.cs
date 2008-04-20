using HWClassLibrary.Debug;

namespace Reni.Parser.TokenClass.Name
{
    internal class TelseT : Base
    {
        /// <summary>
        /// Creates the syntax.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="token">The token.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        /// created 31.03.2007 14:02 on SAPHIRE by HH
        internal override Syntax.SyntaxBase CreateSyntax(Syntax.SyntaxBase left, Token token, Syntax.SyntaxBase right)
        {
            var te = (ThenElse) left;
            Tracer.Assert(te.ElseSyntax == null);
            return new ThenElse(te.CondSyntax, te.ThenToken, te.ThenSyntax, token, right);
        }
    }
}