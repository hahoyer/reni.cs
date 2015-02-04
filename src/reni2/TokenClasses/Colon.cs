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
    sealed class Colon : TokenClass
    {
        protected override Syntax Infix(Syntax left, SourcePart token, Syntax right) => left.CreateDeclarationSyntax(token, right);
    }

    sealed class Exclamation : TokenClass
    {
        public Exclamation(ISubParser<Syntax> parser) { Next = parser; }
        protected override ISubParser<Syntax> Next { get; }
    }

    abstract class DeclarationToken : TokenClass
    {
        protected override Syntax Terminal(SourcePart token) => new DeclarationTokenSyntax(this, token);
        internal abstract Syntax DeclarationSyntax(SourcePart token, CompileSyntax body);
        internal abstract Syntax DefinableSyntax(Definable definable, SourcePart token);
    }

    sealed class ConverterToken : DeclarationToken, ITokenClassWithId
    {
        public const string Id = "converter";
        string ITokenClassWithId.Id => Id;
        internal override Syntax DeclarationSyntax(SourcePart token, CompileSyntax body)
            => new ReniParser.DeclarationSyntax(token, body, isConverter: true);
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
            => new ReniParser.DeclarationSyntax(token, body, isMutable: true);
        internal override Syntax DefinableSyntax(Definable definable, SourcePart token)
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
            => _declaration.DeclarationSyntax(token, right.CheckedToCompiledSyntax(token, RightMustNotBeNullError));

        internal override Syntax SuffixedBy(Definable definable, SourcePart token)
            => _declaration.DefinableSyntax(definable, token);


        IssueId RightMustNotBeNullError()
        {
            NotImplementedMethod();
            return null;
        }
    }
}