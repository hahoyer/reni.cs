using System;
using HWClassLibrary.Debug;

namespace Reni.Parser.TokenClass.Name
{
    [Serializable]

    internal sealed class TelseT : TokenClassBase
    {
        internal override IParsedSyntax CreateSyntax(IParsedSyntax left, Token token, IParsedSyntax right)
        {
            left.AssertIsNotNull();
            right.AssertIsNotNull();
            return left.CreateElseSyntax(token, right.CheckedToCompiledSyntax());
        }
    }
}