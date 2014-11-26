using System;
using System.Collections.Generic;
using System.Linq;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using Reni.TokenClasses;

namespace Reni.ReniParser
{
    sealed class DeclarationTokenFactory : TokenFactory<TokenClass, Syntax>
    {
        public static PrioTable PrioTable
        {
            get
            {
                var prioTable = PrioTable.Left(PrioTable.BeginOfText);
                prioTable += PrioTable.Left("converter");
                prioTable = prioTable.ParenthesisLevelLeft
                    (
                        new[] {"(", "[", "{"},
                        new[] {")", "]", "}"}
                    );
                prioTable += PrioTable.Left(PrioTable.Any);
                prioTable.Correct(PrioTable.Any, PrioTable.BeginOfText, '=');
                prioTable.Correct(")", PrioTable.BeginOfText, '=');
                prioTable.Correct("]", PrioTable.BeginOfText, '=');
                prioTable.Correct("}", PrioTable.BeginOfText, '=');
                return prioTable;
            }
        }

        protected override FunctionCache<string, TokenClass> GetPredefinedTokenClasses()
        {
            return new FunctionCache<string, TokenClass>
            {
                {"converter", new ConverterToken()}
            };
        }

        protected override TokenClass GetEndOfText() { throw new NotImplementedException(); }
        protected override TokenClass GetTokenClass(string name) { throw new NotImplementedException(); }
        protected override TokenClass GetNumber() { throw new NotImplementedException(); }
        protected override TokenClass GetText() { throw new NotImplementedException(); }
        protected override TokenClass GetError(Match.IError message) { return new SyntaxError(message); }
    }
}