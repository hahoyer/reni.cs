using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Parser;
using hw.PrioParser;
using hw.Scanner;
using Reni.ReniParser;
using Reni.Syntax;
using Reni.Validation;

namespace Reni.TokenClasses
{
    sealed class Colon : TokenClass
    {
        protected override ParsedSyntax InfixSyntax(ParsedSyntax left, TokenData token, ParsedSyntax right)
        {
            return left
                .CreateDeclarationSyntax(token, right);
        }
    }

    sealed class Exclamation : TokenClass, IRescannable<IParsedSyntax>
    {
        static readonly ITokenFactory _tokenFactory = DeclarationTokenFactory.Instance;
        //protected override ParsedSyntax TerminalSyntax(TokenData token) { return new ExclamationSyntax(token); }
        Item<IParsedSyntax> IRescannable<IParsedSyntax>.Execute
            (IPart part, SourcePosn sourcePosn, Stack<OpenItem<IParsedSyntax>> stack)
        {
            NotImplementedMethod(part, sourcePosn, stack);
            return null;
        }
    }

    sealed class ConverterToken : TokenClass
    {
        protected override ParsedSyntax SuffixSyntax(ParsedSyntax left, TokenData token)
        {
            return ((DeclarationExtensionSyntax) left)
                .ExtendByConverter(token);
        }
    }

    abstract class DeclarationExtensionSyntax : ParsedSyntax
    {
        protected DeclarationExtensionSyntax(TokenData token)
            : base(token)
        {}

        internal virtual ParsedSyntax ExtendByConverter(TokenData token)
        {
            NotImplementedMethod(token);
            return null;
        }
    }

    sealed class ConverterDeclarationSyntax : DeclarationExtensionSyntax
    {
        readonly TokenData _token;

        internal ConverterDeclarationSyntax(TokenData token, TokenData otherToken)
            : base(token)
        {
            _token = otherToken;
        }

        internal override ParsedSyntax CreateDeclarationSyntax(TokenData token, ParsedSyntax right)
        {
            return new ConverterSyntax
                (
                _token
                ,
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
        internal ExclamationSyntax(TokenData token)
            : base(token)
        {}

        internal override ParsedSyntax ExtendByConverter(TokenData token) { return new ConverterDeclarationSyntax(Token, token); }
    }
}