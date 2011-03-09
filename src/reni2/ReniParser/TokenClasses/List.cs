using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Parser;
using Reni.Syntax;

namespace Reni.ReniParser.TokenClasses
{
    /// <summary>
    ///     List token (comma, semicolon)
    /// </summary>
    internal sealed class List : TokenClass
    {
        internal List() { Name = ","; }
        protected override ParsedSyntax Syntax(ParsedSyntax left, TokenData token, ParsedSyntax right) { return new ListSyntax(left, token, right); }
    }
}