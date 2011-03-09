using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Parser;
using Reni.Syntax;

namespace Reni.TokenClasses
{
    internal sealed class List : TokenClass
    {
        internal List() { Name = ","; }
        protected override ReniParser.ParsedSyntax Syntax(ReniParser.ParsedSyntax left, TokenData token, ReniParser.ParsedSyntax right) { return new ListSyntax(left, token, right); }
    }
}