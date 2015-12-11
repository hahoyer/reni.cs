using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
using Reni.Parser;

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
        internal override SourcePart Token { get; }

        internal override Checked<Syntax> CreateDeclarationSyntax
            (SourcePart token, Syntax right)
        {
            var rightResult = right.ToCompiledSyntax;
            return new DeclarationSyntax(rightResult.Value, Definable).Issues(rightResult.Issues);
        }

        internal override Checked<Syntax> RightSyntax(Syntax right, SourcePart token)
            => Checked<Syntax>.From(ExpressionSyntax.Create(null, Definable, right, token));

        [DisableDump]
        internal override Checked<CompileSyntax> ToCompiledSyntax
            => Checked<CompileSyntax>
                .From(ExpressionSyntax.Create(null, Definable, null, Token));
    }
}