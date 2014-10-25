using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.Struct;

namespace Reni.ReniSyntax
{
    internal sealed class ConverterSyntax : ReniParser.Syntax
    {
        internal readonly CompileSyntax Body;

        internal ConverterSyntax(SourcePart token, CompileSyntax body)
            : base(token) { Body = body; }

        protected override string GetNodeDump() { return "converter (" + Body.NodeDump + ")"; }

        internal override ReniParser.Syntax SurroundedByParenthesis(SourcePart leftToken, SourcePart rightToken) { return Container.Create(leftToken, rightToken, this); }
    }
}