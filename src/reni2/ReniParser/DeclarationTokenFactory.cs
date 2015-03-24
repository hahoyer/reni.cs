using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using hw.Scanner;
using Reni.TokenClasses;

namespace Reni.ReniParser
{
    sealed class DeclarationTokenFactory : TokenFactory
    {
        public static PrioTable PrioTable
        {
            get
            {
                var prioTable = PrioTable.Left(PrioTable.BeginOfText);
                prioTable += PrioTable.Left("converter", "mutable");
                prioTable = prioTable.ParenthesisLevelLeft
                    (
                        new[]
                        {
                            LeftParenthesis.TokenId(1),
                            LeftParenthesis.TokenId(2),
                            LeftParenthesis.TokenId(3)
                        },
                        new[]
                        {
                            RightParenthesis.TokenId(1),
                            RightParenthesis.TokenId(2),
                            RightParenthesis.TokenId(3)
                        }
                    );
                prioTable += PrioTable.Left(PrioTable.Any);
                prioTable.Correct(PrioTable.Any, PrioTable.BeginOfText, '=');
                prioTable.Correct(")", PrioTable.BeginOfText, '=');
                prioTable.Correct("]", PrioTable.BeginOfText, '=');
                prioTable.Correct("}", PrioTable.BeginOfText, '=');
                return prioTable;
            }
        }

        sealed class Unexpected : DeclarationTagToken
        {
            internal override bool IsError => true;
            public override string Id => "<unexpected>";
        }

        protected override ScannerTokenClass GetEndOfText() { throw new NotImplementedException(); }
        protected override ScannerTokenClass GetTokenClass(string name) => new Unexpected();
        protected override ScannerTokenClass GetNumber() { throw new NotImplementedException(); }
        protected override ScannerTokenClass GetText() { throw new NotImplementedException(); }
        protected override ScannerTokenClass GetError(Match.IError message) => new ScannerSyntaxError(message);
    }
}