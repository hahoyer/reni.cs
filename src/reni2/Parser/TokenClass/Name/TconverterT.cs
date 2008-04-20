namespace Reni.Parser.TokenClass.Name
{
    class TconverterT: Base
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
            if (left == null)
                return right.CreateConverterSyntax(token);
            return base.CreateSyntax(left, token, right);
        }
    }
}
