using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using Reni.ReniParser;
using Reni.Syntax;

namespace Reni.TokenClasses
{
    sealed class List : TokenClass
    {
        internal List() { Name = ","; }
        protected override ParsedSyntax InfixSyntax(ParsedSyntax left, TokenData token, ParsedSyntax right) { return new ListSyntax(left, token, right); }
    }
}