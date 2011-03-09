using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Parser;
using Reni.ReniParser;

namespace Reni.TokenClasses
{
    [Serializable]
    internal sealed class ElseToken : TokenClass
    {
        protected override ReniParser.ParsedSyntax Syntax(ReniParser.ParsedSyntax left, TokenData token, ReniParser.ParsedSyntax right)
        {
            left.AssertIsNotNull();
            right.AssertIsNotNull();
            return left.CreateElseSyntax(token, right.CheckedToCompiledSyntax());
        }
    }
}