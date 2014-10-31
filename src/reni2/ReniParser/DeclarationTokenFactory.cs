using System;
using System.Collections.Generic;
using System.Linq;
using hw.Helper;
using hw.Parser;
using Reni.Parser;
using Reni.TokenClasses;

namespace Reni.ReniParser
{
    sealed class DeclarationTokenFactory : TokenFactory<TokenClass, Syntax>
    {
        static readonly DeclarationTokenFactory _instance = new DeclarationTokenFactory();

        static PrioTable PrioTable
        {
            get
            {
                var prioTable = PrioTable.Left(PrioTable.BeginOfText);
                prioTable += PrioTable.Left("converter");
                prioTable = prioTable.ParenthesisLevelRight
                    (
                        new[] { "(", "[", "{" },
                        new[] { ")", "]", "}" }
                    );
                prioTable += PrioTable.Left(PrioTable.Any);
                prioTable.Correct(PrioTable.Any, PrioTable.BeginOfText, '-');
                prioTable.Correct(")", PrioTable.BeginOfText, '-');
                prioTable.Correct("]", PrioTable.BeginOfText, '-');
                prioTable.Correct("}", PrioTable.BeginOfText, '-');
                return prioTable;
            }
        }


        public static readonly IParser<Syntax> ParserInstance = new PrioParser<Syntax>
            (
            PrioTable,
            new Scanner<Syntax>(ReniLexer.Instance),
            _instance
            );

        protected override FunctionCache<string, TokenClass> GetPredefinedTokenClasses()
        {
            return new FunctionCache<string, TokenClass>
            {
                {"converter", new ConverterToken()}
            };
        }

        protected override TokenClass GetSyntaxError(string message) { return new SyntaxError(message); }
    }
}