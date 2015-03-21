using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Parser;
using Reni.ReniParser;
using Reni.ReniSyntax;
using Reni.Validation;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class Colon : TokenClass
    {
        public const string TokenId = ":";
        public override string Id => TokenId;

        protected override Syntax Suffix(Syntax left, IToken token)
            => left.CreateDeclarationSyntax
                (token, new CompileSyntaxError(IssueId.MissingValueInDeclaration));

        protected override Syntax Infix(Syntax left, IToken token, Syntax right)
            => left.CreateDeclarationSyntax(token, right);

        protected override Syntax Terminal(IToken token)
            => new DeclarationSyntax
                (token, new CompileSyntaxError(IssueId.MissingValueInDeclaration));
    }

    [BelongsTo(typeof(MainTokenFactory))]
    sealed class Exclamation : TokenClass
    {
        public const string TokenId = "!";
        public override string Id => TokenId;
        public Exclamation(ISubParser<SourceSyntax> parser) { Next = parser; }
        protected override ISubParser<SourceSyntax> Next { get; }
        protected override Syntax Infix(Syntax left, IToken token, Syntax right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }
        protected override Syntax Terminal(IToken token)
        {
            NotImplementedMethod(token);
            return null;
        }
    }

    [BelongsTo(typeof(DeclarationTokenFactory))]
    abstract class DeclarationTagToken : TokenClass
    {
        internal Syntax DeclarationSyntax(IToken token, CompileSyntax body)
            =>
                new DeclarationSyntax
                    (
                    token,
                    body,
                    new DefinableTokenSyntax(new DeclarationTagSyntax(this))
                    );

        protected override Syntax Terminal(IToken token)
            => new DeclarationTagSyntax(this);

        protected override sealed Syntax Infix(Syntax left, IToken token, Syntax right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }

        [DisableDump]
        internal virtual bool IsKeyword => false;
        [DisableDump]
        internal virtual bool IsError => false;
    }

    sealed class ConverterToken : DeclarationTagToken
    {
        const string TokenId = "converter";
        public override string Id => TokenId;
        internal override bool IsKeyword => true;
    }

    sealed class MutableDeclarationToken : DeclarationTagToken
    {
        const string TokenId = "mutable";
        public override string Id => TokenId;
        internal override bool IsKeyword => true;
    }

    sealed class DeclarationTagSyntax : Syntax
    {
        readonly DeclarationTagToken _tag;

        internal DeclarationTagSyntax(DeclarationTagToken tag) { _tag = tag; }

        [DisableDump]
        internal bool DeclaresMutable => _tag is MutableDeclarationToken;
        [DisableDump]
        internal bool DeclaresConverter => _tag is ConverterToken;
        [DisableDump]
        internal override bool IsKeyword => _tag.IsKeyword;
        [DisableDump]
        internal override bool IsError => _tag.IsError;

        internal override Syntax CreateDeclarationSyntax(IToken token, Syntax right)
            => _tag.DeclarationSyntax
                (token, right.CheckedToCompiledSyntax(RightMustNotBeNullError));

        internal override Syntax SuffixedBy(Definable definable)
            => new DefinableTokenSyntax(definable, this);

        IssueId RightMustNotBeNullError()
        {
            NotImplementedMethod();
            return null;
        }
    }
}