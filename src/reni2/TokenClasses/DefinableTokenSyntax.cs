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
        internal DefinableTokenSyntax(Definable definable, params DeclarationTagSyntax[] tags)
        {
            Tags = tags;
            Definable = definable;
            StopByObjectIds(51);
        }

        internal DefinableTokenSyntax(DefinableTokenSyntax other, DeclarationTagSyntax tag)
            : this(other.Definable, other.Tags.plus(tag)) { }
        public DefinableTokenSyntax(DeclarationTagSyntax definable)
            : this((Definable) null, definable) { }

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
                return new ExpressionSyntax(null, Definable, null);
            }
        }

        [DisableDump]
        protected override IEnumerable<Syntax> DirectChildren => Tags;
    }
}