using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using JetBrains.Annotations;
using Reni.Parser;
using Reni.Syntax;

namespace Reni.ReniParser.TokenClasses
{
    [Serializable]
    internal sealed class Colon : TokenClass
    {
        protected override ParsedSyntax Syntax(ParsedSyntax left, TokenData token, ParsedSyntax right)
        {
            left.AssertIsNotNull();
            return left.CreateDeclarationSyntax(token, right);
        }
    }

    [Serializable]
    internal sealed class Exclamation : TokenClass
    {
        private static readonly ITokenFactory _tokenFactory = DeclarationTokenFactory.Instance;

        [IsDumpEnabled(false)]
        protected override ITokenFactory NewTokenFactory { get { return _tokenFactory; } }

        protected override ParsedSyntax Syntax(ParsedSyntax left, TokenData token, ParsedSyntax right)
        {
            left.AssertIsNull();
            right.AssertIsNull();
            return new ExclamationSyntax(token);
        }
    }

    [Serializable]
    internal sealed class Property : TokenClass
    {
        protected override ParsedSyntax Syntax(ParsedSyntax left, TokenData token, ParsedSyntax right)
        {
            right.AssertIsNull();
            return ((DeclarationExtensionSyntax) left).ExtendByProperty(token);
        }
    }

    [Serializable]
    internal sealed class Converter : TokenClass
    {
        protected override ParsedSyntax Syntax(ParsedSyntax left, TokenData token, ParsedSyntax right)
        {
            right.AssertIsNull();
            return ((DeclarationExtensionSyntax) left).ExtendByConverter(token);
        }
    }

    internal abstract class DeclarationExtensionSyntax : ParsedSyntax
    {
        protected DeclarationExtensionSyntax(TokenData token)
            : base(token) { }

        internal virtual bool IsProperty { get { return false; } }

        internal virtual ParsedSyntax ExtendByProperty(TokenData token)
        {
            NotImplementedMethod(token);
            return null;
        }

        internal virtual ParsedSyntax ExtendByConverter(TokenData token)
        {
            NotImplementedMethod(token);
            return null;
        }
    }

    internal sealed class PropertyDeclarationSyntax : DeclarationExtensionSyntax
    {
        [UsedImplicitly]
        private readonly TokenData _token;

        internal PropertyDeclarationSyntax(TokenData token, TokenData otherToken)
            : base(token) { _token = otherToken; }

        internal override bool IsProperty { get { return true; } }

        internal override ParsedSyntax CreateSyntaxOrDeclaration(Defineable tokenClass, TokenData token, ParsedSyntax right)
        {
            right.AssertIsNull();
            return tokenClass.CreateDeclarationPartSyntax(this, token);
        }
    }

    internal sealed class ConverterDeclarationSyntax : DeclarationExtensionSyntax
    {
        private readonly TokenData _token;

        internal ConverterDeclarationSyntax(TokenData token, TokenData otherToken)
            : base(token) { _token = otherToken; }

        internal override ParsedSyntax CreateDeclarationSyntax(TokenData token, ParsedSyntax right) { return new ConverterSyntax(_token, right.CheckedToCompiledSyntax()); }
    }

    internal sealed class ExclamationSyntax : DeclarationExtensionSyntax
    {
        internal ExclamationSyntax(TokenData token)
            : base(token) { }

        internal override ParsedSyntax ExtendByProperty(TokenData token) { return new PropertyDeclarationSyntax(Token, token); }

        internal override ParsedSyntax ExtendByConverter(TokenData token) { return new ConverterDeclarationSyntax(Token, token); }
    }
}