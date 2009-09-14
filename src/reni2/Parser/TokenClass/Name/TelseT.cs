using System;
using HWClassLibrary.Debug;

namespace Reni.Parser.TokenClass.Name
{
    [Token("else")]
    [Serializable]

    internal sealed class TelseT : TokenClassBase
    {
        internal override IParsedSyntax CreateSyntax(IParsedSyntax left, Token token, IParsedSyntax right)
        {
            ParsedSyntaxExtender.AssertIsNotNull(left);
            ParsedSyntaxExtender.AssertIsNotNull(right);
            return left.CreateElseSyntax(token, ParsedSyntaxExtender.CheckedToCompiledSyntax(right));
        }
    }
}