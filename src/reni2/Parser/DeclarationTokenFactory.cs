﻿using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using hw.Scanner;
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

        protected override IScannerTokenType AnyTokenType { get { throw new NotImplementedException(); } }

        protected override IScannerTokenType TextTokenType { get { throw new NotImplementedException(); } }
        protected override IScannerTokenType NumberTokenType { get { throw new NotImplementedException(); } }
    }
}