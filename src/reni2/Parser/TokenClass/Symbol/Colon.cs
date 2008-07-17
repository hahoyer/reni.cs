using System;
using HWClassLibrary.Debug;
using Reni.Syntax;

namespace Reni.Parser.TokenClass.Symbol
{
    [Token(":")]
    internal sealed class Colon : TokenClassBase
    {
        internal override IParsedSyntax CreateSyntax(IParsedSyntax left, Token token, IParsedSyntax right)
        {
            return left.CreateDeclarationSyntax(token, right);
        }
    }

    [Token("!")]
    internal sealed class Exclamation : TokenClassBase
    {
        private static readonly TokenFactory _tokenFactory = new TokenFactory<DeclarationTokenAttribute>();

        [DumpData(false)]
        internal override TokenFactory NewTokenFactory
        {
            get { return _tokenFactory; }
        }

        internal override IParsedSyntax CreateSyntax(IParsedSyntax left, Token token, IParsedSyntax right)
        {
            ParsedSyntax.IsNull(left);
            ParsedSyntax.IsNull(right);
            return new ExclamationSyntax(token);
        }
    }

    [DeclarationToken("property")]
    internal sealed class Property : TokenClassBase
    {
        internal override IParsedSyntax CreateSyntax(IParsedSyntax left, Token token, IParsedSyntax right)
        {
            ParsedSyntax.IsNull(right);
            return ((DeclarationExtensionSyntax) left).ExtendByProperty(token);
        }
    }

    [DeclarationToken("converter")]
    internal sealed class Converter : TokenClassBase
    {
        internal override IParsedSyntax CreateSyntax(IParsedSyntax left, Token token, IParsedSyntax right)
        {
            ParsedSyntax.IsNull(right);
            return ((DeclarationExtensionSyntax) left).ExtendByConverter(token);
        }
    }

    internal sealed class DeclarationTokenAttribute : TokenAttributeBase
    {
        public DeclarationTokenAttribute(string token) : base(token)
        {
        }

        public DeclarationTokenAttribute() : base(null)
        {
        }

        internal override PrioTable CreatePrioTable()
        {
            var x = PrioTable.LeftAssoc("!");
            x += PrioTable.LeftAssoc("property", "converter");
            x = x.ParLevel
                (new[]
                     {
                         "++-",
                         "+?-",
                         "?--"
                     },
                 new[] {"(", "[", "{", "<frame>"},
                 new[] {")", "]", "}", "<end>"}
                );
            x += PrioTable.LeftAssoc("<else>");
            return x;
        }
    }

    internal abstract class DeclarationExtensionSyntax : ParsedSyntax
    {
        protected DeclarationExtensionSyntax(Token token) : base(token)
        {
        }

        internal virtual bool IsProperty
        {
            get { return false; }
        }

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
        internal new Token Token;

        internal PropertyDeclarationSyntax(Token token, Token otherToken)
            : base(token)
        {
            Token = otherToken;
        }

        internal override bool IsProperty
        {
            get { return true; }
        }

        protected internal override IParsedSyntax CreateSyntax(Token token, IParsedSyntax right)
        {
            IsNull(right);
            return token.TokenClass.CreateDeclarationPartSyntax(this, token);
        }
    }

    internal sealed class ConverterDeclarationSyntax : DeclarationExtensionSyntax
    {
        internal new Token Token;

        internal ConverterDeclarationSyntax(Token token, Token otherToken)
            : base(token)
        {
            Token = otherToken;
        }

        protected internal override IParsedSyntax CreateDeclarationSyntax(Token token, IParsedSyntax right)
        {
            return new ConverterSyntax(Token, ToCompiledSyntax(right));
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