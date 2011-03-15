using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Parser;
using Reni.Struct;

namespace Reni.Syntax
{
    internal sealed class ConverterSyntax : ReniParser.ParsedSyntax
    {
        internal readonly ICompileSyntax Body;

        internal ConverterSyntax(TokenData token, ICompileSyntax body)
            : base(token) { Body = body; }

        internal override string DumpShort() { return "converter (" + Body.DumpShort() + ")"; }

        internal override ReniParser.ParsedSyntax SurroundedByParenthesis(TokenData leftToken, TokenData rightToken) { return Container.Create(leftToken, rightToken, this); }
    }
}