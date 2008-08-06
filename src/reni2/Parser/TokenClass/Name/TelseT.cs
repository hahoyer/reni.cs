using System;
using HWClassLibrary.Debug;

namespace Reni.Parser.TokenClass.Name
{
    [Token("else")]
    [Serializable]

    internal class TelseT : TokenClassBase
    {
        internal override IParsedSyntax CreateSyntax(IParsedSyntax left, Token token, IParsedSyntax right)
        {
            ParsedSyntax.IsNotNull(left);
            ParsedSyntax.IsNotNull(right);
            return left.CreateElseSyntax(token, ParsedSyntax.ToCompiledSyntax(right));
        }
    }
}