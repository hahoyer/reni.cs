using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Scanner;
using Reni.ReniParser;
using Reni.ReniSyntax;

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

        internal override bool IsIdentifier => true;

        internal override Syntax CreateDeclarationSyntax
            (SourcePart token, Syntax right)
            => new DeclarationSyntax(right.ToCompiledSyntax, this);

        [DisableDump]
        internal override CompileSyntax ToCompiledSyntax
            => new ExpressionSyntax(null, Definable, null, Token);
    }
}