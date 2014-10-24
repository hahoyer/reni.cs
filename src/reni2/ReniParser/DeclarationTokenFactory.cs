using System;
using System.Collections.Generic;
using System.Linq;
using hw.Helper;
using hw.PrioParser;
using Reni.TokenClasses;

namespace Reni.ReniParser
{
    sealed class DeclarationTokenFactory : hw.Parser.TokenFactory<TokenClass>
    {
        DeclarationTokenFactory()
            : base(PrioTable) { }
        internal static DeclarationTokenFactory Instance { get { return new DeclarationTokenFactory(); } }

        static PrioTable PrioTable
        {
            get
            {
                var prioTable = PrioTable.Left(PrioTable.BeginOfText);
                prioTable += PrioTable.Left("converter");
                prioTable = prioTable.ParenthesisLevel
                    (
                        new[] {"(", "[", "{"},
                        new[] {")", "]", "}"}
                    );
                prioTable += PrioTable.Left(PrioTable.Any);
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

        protected override TokenClass GetSyntaxError(string message) { return new SyntaxError(message); }
    }
}