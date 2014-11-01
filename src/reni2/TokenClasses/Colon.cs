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
        protected override Syntax Infix(Syntax left, SourcePart token, Syntax right)
        {
            return left
                .CreateDeclarationSyntax(token, right);
        }
    }

    sealed class Exclamation : TokenClass
    {
        readonly ISubParser<Syntax> _parser;
        public Exclamation(ISubParser<Syntax> parser) { _parser = parser; }
        protected override Syntax TerminalSyntax(SourcePart token) { return new ExclamationSyntax(token); }
        protected override ISubParser<Syntax> Next { get { return _parser; } }
    }

    sealed class ConverterToken : TokenClass
    {
        protected override Syntax Terminal(SourcePart token) { return new ConverterDeclarationSyntax(token, token); }

        protected override Syntax SuffixSyntax(Syntax left, SourcePart token)
        {
            return ((DeclarationExtensionSyntax) left)
                .ExtendByConverter(token);
        }
    }

    abstract class DeclarationExtensionSyntax : Syntax
    {
        protected DeclarationExtensionSyntax(SourcePart token)
            : base(token) { }

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
            : base(token) { _token = otherToken; }

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
            : base(token) { }

        internal override Syntax ExtendByConverter(SourcePart token) { return new ConverterDeclarationSyntax(Token, token); }
    }
}