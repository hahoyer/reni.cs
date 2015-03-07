using System;
using System.Collections.Generic;
using System.Linq;
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
    abstract class DeclarationToken : TokenClass
    {
        protected override Syntax Terminal(SourcePart token)
            => new DeclarationTokenSyntax(this, token);
        internal abstract Syntax DeclarationSyntax(SourcePart token, CompileSyntax body);
        internal abstract Syntax DefinableSyntax(Definable definable, SourcePart token);

        protected override sealed Syntax Infix(Syntax left, SourcePart token, Syntax right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }
    }

    sealed class ConverterToken : DeclarationToken, ITokenClassWithId
    {
        public const string Id = "converter";
        string ITokenClassWithId.Id => Id;
        internal override Syntax DeclarationSyntax(SourcePart token, CompileSyntax body)
            => new DeclarationSyntax(token, body, isConverter: true);
        internal override Syntax DefinableSyntax(Definable definable, SourcePart token)
        {
            NotImplementedMethod(definable, token);
            return null;
        }
    }

    sealed class MutableDeclarationToken : DeclarationToken, ITokenClassWithId
    {
        public const string Id = "mutable";
        string ITokenClassWithId.Id => Id;
        internal override Syntax DeclarationSyntax(SourcePart token, CompileSyntax body)
            => new DeclarationSyntax(token, body, DefinableTokenSyntax(null, token));
        internal override Syntax DefinableSyntax(Definable definable, SourcePart token)
            => DefinableTokenSyntax(definable, token);
        static DefinableTokenSyntax DefinableTokenSyntax(Definable definable, SourcePart token)
            => new DefinableTokenSyntax(definable, token, true);
    }

    sealed class DeclarationTokenSyntax : Syntax
    {
        readonly DeclarationToken _declaration;

        internal DeclarationTokenSyntax(DeclarationToken declaration, SourcePart token)
            : base(token)
        {
            _declaration = declaration;
        }

        internal override Syntax CreateDeclarationSyntax(SourcePart token, Syntax right)
            =>
                _declaration.DeclarationSyntax
                    (token, right.CheckedToCompiledSyntax(token, RightMustNotBeNullError));

        internal override Syntax SuffixedBy(Definable definable, SourcePart token)
            => _declaration.DefinableSyntax(definable, token);


        IssueId RightMustNotBeNullError()
        {
            NotImplementedMethod();
            return null;
        }
    }
}