using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Parser;

namespace Reni.ReniParser.TokenClasses
{
    [Serializable]
    internal sealed class ElseToken : TokenClass
    {
        protected override ParsedSyntax Syntax(ParsedSyntax left, TokenData token, ParsedSyntax right)
        {
            left.AssertIsNotNull();
            right.AssertIsNotNull();
            return left.CreateElseSyntax(token, right.CheckedToCompiledSyntax());
        }
    }
}