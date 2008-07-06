namespace Reni.Parser.TokenClass.Name
{
    internal sealed class TthenT : TokenClassBase
    {
        internal static string DumpShort()
        {
            return "then";
        }

        internal override IParsedSyntax CreateSyntax(IParsedSyntax left, Token token, IParsedSyntax right)
        {
            ParsedSyntax.IsNotNull(right);
            return right.CreateThenSyntax(token, ParsedSyntax.ToCompiledSyntax(left));
        }
    }
}