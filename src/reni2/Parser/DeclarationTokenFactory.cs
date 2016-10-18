using System;
using System.Collections.Generic;
using System.Linq;
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
                        LeftParenthesis.TokenId(0)
                    },
                    new[]
                    {
                        RightParenthesis.TokenId(3),
                        RightParenthesis.TokenId(2),
                        RightParenthesis.TokenId(1),
                        RightParenthesis.TokenId(0)
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
        {
            throw new NotImplementedException();
        }
    }
}