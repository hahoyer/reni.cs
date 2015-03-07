using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Scanner;
using Reni.ReniParser;
using Reni.ReniSyntax;

namespace Reni.TokenClasses
{
    sealed class DefinableTokenSyntax : Syntax
    {
        internal DefinableTokenSyntax(Definable definable, SourcePart tokenData, bool isMutable)
            : base(tokenData)
        {
            IsMutable = isMutable;
            Definable = definable;
        }

        internal bool IsMutable { get; }
        internal Definable Definable { get; }

        internal override Syntax CreateDeclarationSyntax(SourcePart token, Syntax right)
            => new DeclarationSyntax(token, right.ToCompiledSyntax, this);

        [DisableDump]
        internal override CompileSyntax ToCompiledSyntax
        {
            get
            {
                Tracer.Assert(!IsMutable);
                return new ExpressionSyntax(Definable, null, Token, null);
            }
        }
    }
}