using System;
using HWClassLibrary.Debug;

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
            return new DeclarationExtensionSyntax(token);
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

    internal sealed class DeclarationExtensionSyntax : ParsedSyntax
    {
        internal readonly Token PropertyToken;

        public DeclarationExtensionSyntax(Token token) : base(token)
        {
        }

        private DeclarationExtensionSyntax(Token token, Token propertyToken) : base(token)
        {
            PropertyToken = propertyToken;
        }

        internal IParsedSyntax ExtendByProperty(Token token)
        {
            Tracer.Assert(PropertyToken == null);
            return new DeclarationExtensionSyntax(Token, token);
        }

        protected internal override IParsedSyntax CreateSyntax(Token token, IParsedSyntax right)
        {
            Tracer.Assert(right == null);
            return token.TokenClass.CreateDeclarationPartSyntax(this,token);
        }
    }
}