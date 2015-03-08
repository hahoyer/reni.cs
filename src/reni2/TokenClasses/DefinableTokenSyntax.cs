using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Parser;
using Reni.ReniParser;
using Reni.ReniSyntax;

namespace Reni.TokenClasses
{
    sealed class DefinableTokenSyntax : Syntax
    {
        internal DefinableTokenSyntax
            (
            Definable definable,
            Token tokenData,
            DeclarationTagSyntax tag = null)
            : base(tokenData)
        {
            Tag = tag;
            Definable = definable;
        }
        DefinableTokenSyntax(DefinableTokenSyntax other, ParsedSyntax[] parts)
            : base(other, parts)
        {
            Tag = other.Tag;
            Definable = other.Definable;
        }

        internal bool IsConverter => Tag?.DeclaresConverter ?? false;
        internal bool IsMutable => Tag?.DeclaresMutable ?? false;
        internal DeclarationTagSyntax Tag { get; }
        internal Definable Definable { get; }

        internal override Syntax CreateDeclarationSyntax(Token token, Syntax right)
            => new DeclarationSyntax(token, right.ToCompiledSyntax, this);

        [DisableDump]
        internal override CompileSyntax ToCompiledSyntax
        {
            get
            {
                Tracer.Assert(Tag == null);
                return new ExpressionSyntax(Definable, null, Token, null);
            }
        }
        protected override IEnumerable<Syntax> SyntaxChildren { get { yield return Tag; } }
        internal override ReniParser.Syntax Surround(params ParsedSyntax[] parts)
            => new DefinableTokenSyntax(this, parts);
    }
}