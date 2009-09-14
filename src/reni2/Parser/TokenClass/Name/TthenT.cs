using System;
using HWClassLibrary.Debug;

namespace Reni.Parser.TokenClass.Name
{
    [Token("then")]
    [Serializable]
    internal sealed class TthenT : TokenClassBase
    {
        internal override IParsedSyntax CreateSyntax(IParsedSyntax left, Token token, IParsedSyntax right)
        {
            ParsedSyntaxExtender.AssertIsNotNull(right);
            return right.CreateThenSyntax(token, ParsedSyntaxExtender.CheckedToCompiledSyntax(left));
        }
    }
}