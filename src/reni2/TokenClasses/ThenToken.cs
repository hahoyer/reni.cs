using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Parser;
using Reni.ReniParser;

namespace Reni.TokenClasses
{
    [Serializable]
    internal sealed class ThenToken : TokenClass
    {
        protected override ReniParser.ParsedSyntax Syntax(ReniParser.ParsedSyntax left, TokenData token, ReniParser.ParsedSyntax right)
        {
            right.AssertIsNotNull();
            return right.CreateThenSyntax(token, left.CheckedToCompiledSyntax());
        }
    }
}