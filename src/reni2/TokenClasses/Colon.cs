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
        protected override Syntax InfixSyntax(Syntax left, SourcePart token, Syntax right)
        {
            return left
                .CreateDeclarationSyntax(token, right);
        }
    }

    sealed class Exclamation : TokenClass
    {
        static readonly ITokenFactory<Syntax> _tokenFactory = DeclarationTokenFactory.Instance;
        protected override Syntax TerminalSyntax(SourcePart token) { return new ExclamationSyntax(token); }
    }

    sealed class ConverterToken : TokenClass
    {
        protected override Syntax TerminalSyntax(SourcePart token)
        {
            return new ConverterDeclarationSyntax(token, token);
        }

        protected override Syntax SuffixSyntax(Syntax left, SourcePart token)
        {
            return ((DeclarationExtensionSyntax) left)
                .ExtendByConverter(token);
        }
    }

    abstract class DeclarationExtensionSyntax : Syntax
    {
        protected DeclarationExtensionSyntax(SourcePart token)
            : base(token)
        {}

        internal virtual Syntax ExtendByConverter(SourcePart token)
        {
            NotImplementedMethod(token);
            return null;
        }
    }

    sealed class ConverterDeclarationSyntax : DeclarationExtensionSyntax
    {
        readonly SourcePart _token;

        internal ConverterDeclarationSyntax(SourcePart token, SourcePart otherToken)
            : base(token)
        {
            _token = otherToken;
        }

        internal override Syntax CreateDeclarationSyntax(SourcePart token, Syntax right)
        {
            return new ConverterSyntax
                (
                _token,
                right.CheckedToCompiledSyntax(token, RightMustNotBeNullError)
                );
        }
        IssueId RightMustNotBeNullError()
        {
            NotImplementedMethod();
            return null;
        }
    }

    sealed class ExclamationSyntax : DeclarationExtensionSyntax
    {
        internal ExclamationSyntax(SourcePart token)
            : base(token)
        {}

        internal override Syntax ExtendByConverter(SourcePart token) { return new ConverterDeclarationSyntax(Token, token); }
    }
}