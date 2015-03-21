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
        internal DefinableTokenSyntax(Definable definable, SourcePart token, params DeclarationTagSyntax[] tags)
        {
            Tags = tags;
            Definable = definable;
            Token = token;
            StopByObjectIds(51);
        }

        internal DefinableTokenSyntax(DefinableTokenSyntax other, DeclarationTagSyntax tag, SourcePart token)
            : this(other.Definable, token, other.Tags.plus(tag)) { }
        public DefinableTokenSyntax(DeclarationTagSyntax definable, SourcePart token)
            : this((Definable) null, token, definable) { }

        internal DeclarationTagSyntax[] Tags { get; }
        internal Definable Definable { get; }
        SourcePart Token { get; }

        internal override bool IsIdentifier => true;
        [DisableDumpExcept(true)]
        internal bool IsConverter => Tags.Any(item => item.DeclaresConverter);
        [DisableDumpExcept(true)]
        internal bool IsMutable => Tags.Any(item => item.DeclaresMutable);

        internal override Syntax CreateDeclarationSyntax(IToken token, Syntax right)
            => new DeclarationSyntax(token, right.ToCompiledSyntax, this);

        [DisableDump]
        internal override CompileSyntax ToCompiledSyntax
        {
            get
            {
                Tracer.Assert(!Tags.Any());
                return new ExpressionSyntax(null, Definable, null,Token);
            }
        }

        [DisableDump]
        protected override IEnumerable<Syntax> DirectChildren => Tags;
    }
}