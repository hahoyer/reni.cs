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
            IToken tokenData)
            : base(tokenData)
        {
            Tags = new DeclarationTagSyntax[0];
            Definable = definable;
        }
        internal DefinableTokenSyntax
            (
            DefinableTokenSyntax other,
            DeclarationTagSyntax tag)
            : base(other.Token)
        {
            Tags = other.Tags.plus(tag);
            Definable = other.Definable;
        }

        internal override bool IsIdentifier => true;
        [DisableDumpExcept(true)]
        internal bool IsConverter => Tags.Any(item => item.DeclaresConverter);
        [DisableDumpExcept(true)]
        internal bool IsMutable => Tags.Any(item => item.DeclaresMutable);
        internal DeclarationTagSyntax[] Tags { get; }
        internal Definable Definable { get; }

        internal override Syntax CreateDeclarationSyntax(IToken token, Syntax right)
            => new DeclarationSyntax(token, right.ToCompiledSyntax, this);

        [DisableDump]
        internal override CompileSyntax ToCompiledSyntax
        {
            get
            {
                Tracer.Assert(!Tags.Any());
                return new ExpressionSyntax(null, this, null);
            }
        }

        [DisableDump]
        protected override IEnumerable<Syntax> DirectChildren => Tags;
    }
}