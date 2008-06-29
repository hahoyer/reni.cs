using Reni.Syntax;

namespace Reni.Parser.TokenClass.Symbol
{
    /// <summary>
    /// Summary description for Colon.
    /// </summary>
    internal sealed class Colon : TokenClassBase
    {
        internal override IParsedSyntax CreateSyntax(IParsedSyntax left, Token token, IParsedSyntax right)
        {
            return left.CreateDeclarationSyntax(token, right);
        }
    }
}