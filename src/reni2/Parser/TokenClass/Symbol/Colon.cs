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
        static private readonly TokenFactory _tokenFactory = new TokenFactory<DeclarationTokenAttribute>();
        [DumpData(false)]
        internal override TokenFactory NewTokenFactory { get { return _tokenFactory; } }
    }

    [DeclarationToken("property")]
    internal sealed class Property : TokenClassBase
    {
    }

    [DeclarationToken("converter")]
    internal sealed class Converter : TokenClassBase
    {
    }

    internal class DeclarationTokenAttribute : TokenAttributeBase
    {
        public DeclarationTokenAttribute(string token) : base(token) {}

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
                       new[] { "(", "[", "{", "<frame>" },
                       new[] { ")", "]", "}", "<end>" }
                   );
            x += PrioTable.LeftAssoc("<else>");
            return x;
        }
    }
}