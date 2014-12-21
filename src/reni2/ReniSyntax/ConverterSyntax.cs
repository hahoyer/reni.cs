using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Scanner;
using Reni.ReniParser;
using Reni.TokenClasses;

namespace Reni.ReniSyntax
{
    sealed class ConverterSyntax : Syntax
    {
        internal readonly CompileSyntax Body;

        internal ConverterSyntax(SourcePart token, CompileSyntax body)
            : base(token) { Body = body; }

        protected override string GetNodeDump() { return "converter (" + Body.NodeDump + ")"; }

        [DisableDump]
        internal override CompileSyntax ContainerStatementToCompileSyntax { get { return Body.ContainerStatementToCompileSyntax; } }
        [DisableDump]
        internal override CompileSyntax ToCompiledSyntax { get { return ToContainer; } }
    }
}