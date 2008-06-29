using Reni.Syntax;

namespace Reni.Parser.TokenClass.Name
{
    internal class TconverterT : TokenClassBase
    {
        /// <summary>
        /// Creates the syntax.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="token">The token.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        /// created 31.03.2007 14:02 on SAPHIRE by HH
        internal override IParsedSyntax CreateSyntax(IParsedSyntax left, Token token, IParsedSyntax right)
        {   
            ParsedSyntax.IsNull(left);
            return new ConverterSyntax(token, ParsedSyntax.ToCompiledSyntax(right));
        }
    }
}