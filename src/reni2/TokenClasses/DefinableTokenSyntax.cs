using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Parser;
using hw.Scanner;
using Reni.ReniParser;
using Reni.ReniSyntax;

namespace Reni.TokenClasses
{
    sealed class DefinableTokenSyntax : Syntax
    {
        internal DefinableTokenSyntax
            (
            Definable definable,
            SourcePart tokenData,
            DeclarationTagSyntax tag = null,
            SourcePart additionalSourcePart = null)
            : base(tokenData, additionalSourcePart)
        {
            Tag = tag;
            Definable = definable;
        }

        internal bool IsConverter => Tag?.DeclaresConverter ?? false;
        internal bool IsMutable => Tag?.DeclaresMutable ?? false;
        internal DeclarationTagSyntax Tag { get; }
        internal Definable Definable { get; }

        internal override Syntax CreateDeclarationSyntax(SourcePart token, Syntax right)
            => new DeclarationSyntax(token, right.ToCompiledSyntax, this);

        [DisableDump]
        internal override CompileSyntax ToCompiledSyntax
        {
            get
            {
                Tracer.Assert(Tag==null);
                return new ExpressionSyntax(Definable, null, Token, null);
            }
        }
        protected override ParsedSyntax[] Children => new ParsedSyntax[] { Tag};
    }
}