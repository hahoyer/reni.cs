using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Scanner;
using Reni.ReniParser;

namespace Reni.TokenClasses
{
    sealed class DefinableSyntax : Syntax
    {
        internal DefinableSyntax
            (SourcePart token, Definable definable)
        {
            Definable = definable;
            Token = token;
            StopByObjectIds();
        }

        internal Definable Definable { get; }
        SourcePart Token { get; }

        internal override Checked<Syntax> CreateDeclarationSyntax
            (SourcePart token, Syntax right)
        {
            var rightResult = right.ToCompiledSyntax;
            return new DeclarationSyntax(rightResult.Value, Definable).Issues(rightResult.Issues);
        }

        [DisableDump]
        internal override Checked<CompileSyntax> ToCompiledSyntax
            => new ExpressionSyntax(null, Definable, null, Token);
    }
}