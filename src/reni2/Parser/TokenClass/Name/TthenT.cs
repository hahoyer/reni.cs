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
            ParsedSyntax.IsNotNull(right);
            return right.CreateThenSyntax(token, ParsedSyntax.ToCompiledSyntax(left));
        }
    }
}