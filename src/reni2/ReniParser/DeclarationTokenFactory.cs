using System;
using System.Collections.Generic;
using System.Linq;
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
                prioTable += PrioTable.Left("converter", "mutable");
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

        protected override IDictionary<string, TokenClass> GetPredefinedTokenClasses()
        {
            return new ITokenClassWithId[]
            {
                new ConverterToken(),
                new MutableDeclarationToken()
            }
                .ToDictionary(t => t.Id, t => (TokenClass) t);
        }

        protected override TokenClass GetEndOfText() { throw new NotImplementedException(); }
        protected override TokenClass GetTokenClass(string name) { throw new NotImplementedException(); }
        protected override TokenClass GetNumber() { throw new NotImplementedException(); }
        protected override TokenClass GetText() { throw new NotImplementedException(); }
        protected override TokenClass GetError(Match.IError message) { return new SyntaxError(message); }
    }
}