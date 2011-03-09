using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Parser;

namespace Reni.ReniParser.TokenClasses
{
    [Serializable]
    internal sealed class ThenToken : TokenClass
    {
        protected override ParsedSyntax Syntax(ParsedSyntax left, TokenData token, ParsedSyntax right)
        {
            right.AssertIsNotNull();
            return right.CreateThenSyntax(token, left.CheckedToCompiledSyntax());
        }
    }
}