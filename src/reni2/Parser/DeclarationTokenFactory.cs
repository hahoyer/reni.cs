using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using Reni.TokenClasses;

namespace Reni.Parser
{
    sealed class DeclarationTokenFactory : TokenFactory
    {
        public static PrioTable PrioTable
        {
            get
            {
                var prioTable = PrioTable.Left(PrioTable.Any);
                prioTable += PrioTable.BracketParallels
                (
                    new[]
                    {
                        LeftParenthesis.TokenId(3),
                        LeftParenthesis.TokenId(2),
                        LeftParenthesis.TokenId(1),
                        PrioTable.BeginOfText
                    },
                    new[]
                    {
                        RightParenthesisBase.TokenId(3),
                        RightParenthesisBase.TokenId(2),
                        RightParenthesisBase.TokenId(1),
                        PrioTable.EndOfText
                    }
                );
                return prioTable;
            }
        }

        sealed class Unexpected : DeclarationTagToken
        {
            public override string Id => "<unexpected>";
        }

        internal override IParserTokenType<Syntax> GetTokenClass(string name)
            => new InvalidDeclarationError(name);
    }

    sealed class InvalidDeclarationError : DumpableObject, IParserTokenType<Syntax>, ITokenClass
    {
        readonly string Name;
        public InvalidDeclarationError(string name) { Name = name; }

        Syntax IParserTokenType<Syntax>.Create(Syntax left, IToken token, Syntax right)
            => Syntax.CreateSourceSyntax(left, this, token, right);

        string IParserTokenType<Syntax>.PrioTableId => Name;
        string ITokenClass.Id => Name;
    }
}