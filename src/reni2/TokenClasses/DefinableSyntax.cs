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
            (SourcePart token, Definable definable, params DeclarationTagToken[] tags)
        {
            Tags = tags;
            Definable = definable;
            Token = token;
            StopByObjectIds();
        }

        internal DefinableSyntax
            (SourcePart token, DefinableSyntax other, DeclarationTagToken tag)
            : this(token, other.Definable, other.Tags.plus(tag)) { }

        public DefinableSyntax(SourcePart token, DeclarationTagToken tag)
            : this(token, (Definable) null, tag) { }

        internal DeclarationTagToken[] Tags { get; }
        internal Definable Definable { get; }
        SourcePart Token { get; }

        internal override bool IsIdentifier => true;

        [DisableDumpExcept(true)]
        internal bool IsConverter => Tags.Any(item => item.DeclaresConverter);
        [DisableDumpExcept(true)]
        internal bool IsMutable => Tags.Any(item => item.DeclaresMutable);

        internal override Syntax ExclamationSyntax(SourcePart token)
            => new DefinableTagsToken(Tags, token);

        sealed class DefinableTagsToken : Syntax
        {
            public DeclarationTagToken[] Tags { get; set; }
            public SourcePart Token { get; set; }

            public DefinableTagsToken(DeclarationTagToken[] tags, SourcePart token)
            {
                Tags = tags;
                Token = token;
            }

            internal override CompileSyntax ToCompiledSyntax
            {
                get
                {
                    NotImplementedMethod();
                    return null;
                }
            }
        }

        internal override Syntax CreateDeclarationSyntax(SourcePart token, Syntax right)
            => new DeclarationSyntax(token, right.ToCompiledSyntax, this);

        [DisableDump]
        internal override CompileSyntax ToCompiledSyntax
        {
            get
            {
                Tracer.Assert(!Tags.Any());
                return new ExpressionSyntax(null, Definable, null, Token);
            }
        }
    }
}