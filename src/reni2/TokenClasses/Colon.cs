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

    sealed class ConverterToken : TokenClass
    {
        protected override Syntax Terminal(SourcePart token) => new ConverterDeclarationSyntax(token, token);
    }

    abstract class DeclarationExtensionSyntax : Syntax
    {
        protected DeclarationExtensionSyntax(SourcePart token)
            : base(token) { }
    }

    sealed class ConverterDeclarationSyntax : DeclarationExtensionSyntax
    {
        readonly SourcePart _token;

        internal ConverterDeclarationSyntax(SourcePart token, SourcePart otherToken)
            : base(token) { _token = otherToken; }

        internal override Syntax CreateDeclarationSyntax(SourcePart token, Syntax right)
            => new ConverterSyntax(_token, right.CheckedToCompiledSyntax(token, RightMustNotBeNullError));

        IssueId RightMustNotBeNullError()
        {
            NotImplementedMethod();
            return null;
        }
    }
}