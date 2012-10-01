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
        internal readonly CompileSyntax Body;

        internal ConverterSyntax(TokenData token, CompileSyntax body)
            : base(token) { Body = body; }

        protected override string GetNodeDump() { return "converter (" + Body.NodeDump + ")"; }

        internal override ReniParser.ParsedSyntax SurroundedByParenthesis(TokenData leftToken, TokenData rightToken) { return Container.Create(leftToken, rightToken, this); }
    }
}