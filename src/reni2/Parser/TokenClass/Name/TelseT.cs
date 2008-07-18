namespace Reni.Parser.TokenClass.Name
{
    [Token("else")]
    internal class TelseT : TokenClassBase
    {
        internal static string DumpShort()
        {
            return "else";
        }

        internal override IParsedSyntax CreateSyntax(IParsedSyntax left, Token token, IParsedSyntax right)
        {
            ParsedSyntax.IsNotNull(left);
            ParsedSyntax.IsNotNull(right);
            return left.CreateElseSyntax(token, ParsedSyntax.ToCompiledSyntax(right));
        }
    }
}