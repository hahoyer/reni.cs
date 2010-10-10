using System;
using HWClassLibrary.Debug;
using JetBrains.Annotations;
using Reni.Syntax;

namespace Reni.Parser.TokenClass
{
    [Serializable]
    internal sealed class Colon : TokenClassBase
    {
        internal override IParsedSyntax CreateSyntax(IParsedSyntax left, Token token, IParsedSyntax right)
        {
            return left.CreateDeclarationSyntax(token, right);
        }
    }

    [Serializable]
    internal sealed class Exclamation : TokenClassBase
    {
        private static readonly TokenFactory _tokenFactory = DeclarationTokenFactory.Instance;

        [IsDumpEnabled(false)]
        internal override TokenFactory NewTokenFactory { get { return _tokenFactory; } }

        internal override IParsedSyntax CreateSyntax(IParsedSyntax left, Token token, IParsedSyntax right)
        {
            left.AssertIsNull();
            right.AssertIsNull();
            return new ExclamationSyntax(token);
        }
    }

    [Serializable]
    internal sealed class Property : TokenClassBase
    {
        internal override IParsedSyntax CreateSyntax(IParsedSyntax left, Token token, IParsedSyntax right)
        {
            right.AssertIsNull();
            return ((DeclarationExtensionSyntax) left).ExtendByProperty(token);
        }
    }

    [Serializable]
    internal sealed class Converter : TokenClassBase
    {
        internal override IParsedSyntax CreateSyntax(IParsedSyntax left, Token token, IParsedSyntax right)
        {
            right.AssertIsNull();
            return ((DeclarationExtensionSyntax) left).ExtendByConverter(token);
        }
    }

    internal abstract class DeclarationExtensionSyntax : ParsedSyntax
    {
        protected DeclarationExtensionSyntax(Token token)
            : base(token)
        {
        }

        internal virtual bool IsProperty { get { return false; } }

        internal virtual IParsedSyntax ExtendByProperty(Token token)
        {
            NotImplementedMethod(token);
            return null;
        }

        internal virtual IParsedSyntax ExtendByConverter(Token token)
        {
            NotImplementedMethod(token);
            return null;
        }
    }

    internal sealed class PropertyDeclarationSyntax : DeclarationExtensionSyntax
    {
        [UsedImplicitly]
        private readonly Token _token;

        internal PropertyDeclarationSyntax(Token token, Token otherToken)
            : base(token)
        {
            _token = otherToken;
        }

        internal override bool IsProperty { get { return true; } }

        protected override IParsedSyntax CreateSyntaxOrDeclaration(Token token, IParsedSyntax right)
        {
            right.AssertIsNull();
            return token.TokenClass.CreateDeclarationPartSyntax(this, token);
        }
    }

    internal sealed class ConverterDeclarationSyntax : DeclarationExtensionSyntax
    {
        private readonly Token _token;

        internal ConverterDeclarationSyntax(Token token, Token otherToken)
            : base(token)
        {
            _token = otherToken;
        }

        protected override IParsedSyntax CreateDeclarationSyntax(Token token, IParsedSyntax right)
        {
            return new ConverterSyntax(_token, right.CheckedToCompiledSyntax());
        }
    }

    internal sealed class ExclamationSyntax : DeclarationExtensionSyntax
    {
        internal ExclamationSyntax(Token token)
            : base(token)
        {
        }

        internal override IParsedSyntax ExtendByProperty(Token token)
        {
            return new PropertyDeclarationSyntax(Token, token);
        }

        internal override IParsedSyntax ExtendByConverter(Token token)
        {
            return new ConverterDeclarationSyntax(Token, token);
        }
    }
}