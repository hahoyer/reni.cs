using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Parser;
using hw.Scanner;
using Reni.ReniParser;
using Reni.ReniSyntax;
using Reni.Validation;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class Colon : TokenClass, ITokenClassWithId
    {
        public const string Id = ":";
        string ITokenClassWithId.Id => Id;
        protected override Syntax Infix(Syntax left, SourcePart token, Syntax right)
            => left.CreateDeclarationSyntax(token, right);
        protected override Syntax Terminal(SourcePart token)
        {
            NotImplementedMethod(token);
            return null;
        }
    }

    [BelongsTo(typeof(MainTokenFactory))]
    sealed class Exclamation : TokenClass, ITokenClassWithId
    {
        public const string Id = "!";
        string ITokenClassWithId.Id => Id;
        public Exclamation(ISubParser<Syntax> parser) { Next = parser; }
        protected override ISubParser<Syntax> Next { get; }
        protected override Syntax Infix(Syntax left, SourcePart token, Syntax right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }
        protected override Syntax Terminal(SourcePart token)
        {
            NotImplementedMethod(token);
            return null;
        }
    }

    [BelongsTo(typeof(DeclarationTokenFactory))]
    abstract class DeclarationTagToken : TokenClass
    {
        internal Syntax DeclarationSyntax(SourcePart token, CompileSyntax body)
            =>
                new DeclarationSyntax
                    (
                    token,
                    body,
                    new DeclarationTagSyntax(this, token).DefinableTokenSyntax(null, token)
                    );

        protected override Syntax Terminal(SourcePart token)
            => new DeclarationTagSyntax(this, token);

        protected override sealed Syntax Infix(Syntax left, SourcePart token, Syntax right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }

        [DisableDump]
        internal virtual bool IsKeyword => false;
        [DisableDump]
        internal virtual bool IsError => false;
    }

    sealed class ConverterToken : DeclarationTagToken, ITokenClassWithId
    {
        public const string Id = "converter";
        string ITokenClassWithId.Id => Id;
        internal override bool IsKeyword => true;
    }

    sealed class MutableDeclarationToken : DeclarationTagToken, ITokenClassWithId
    {
        public const string Id = "mutable";
        string ITokenClassWithId.Id => Id;
        internal override bool IsKeyword => true;
    }

    sealed class DeclarationTagSyntax : Syntax
    {
        readonly DeclarationTagToken _tag;

        internal DeclarationTagSyntax
            (DeclarationTagToken tag, SourcePart token, SourcePart additionalSourcePart = null)
            : base(token, additionalSourcePart) { _tag = tag; }

        [DisableDump]
        internal bool DeclaresMutable => _tag is MutableDeclarationToken;
        [DisableDump]
        internal bool DeclaresConverter => _tag is ConverterToken;
        [DisableDump]
        internal override bool IsKeyword => _tag.IsKeyword;
        [DisableDump]
        internal override bool IsError => _tag.IsError;

        internal override Syntax CreateDeclarationSyntax(SourcePart token, Syntax right)
            =>
                _tag.DeclarationSyntax
                    (token, right.CheckedToCompiledSyntax(token, RightMustNotBeNullError));

        internal override Syntax Sourround(SourcePart sourcePart)
            => new DeclarationTagSyntax(_tag, Token, sourcePart);

        internal override Syntax SuffixedBy(Definable definable, SourcePart token)
            => DefinableTokenSyntax(definable, token);


        IssueId RightMustNotBeNullError()
        {
            NotImplementedMethod();
            return null;
        }
        internal DefinableTokenSyntax DefinableTokenSyntax
            (Definable definable, SourcePart token, SourcePart additionalSourcePart = null)
            =>
                new DefinableTokenSyntax
                    (
                    definable,
                    token,
                    this,
                    additionalSourcePart);
    }
}