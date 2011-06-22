using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Parser;
using Reni.TokenClasses;

namespace Reni.ReniParser
{
    internal sealed class DeclarationTokenFactory : Parser.TokenFactory<TokenClasses.TokenClass>
    {
        internal static DeclarationTokenFactory Instance { get { return new DeclarationTokenFactory(); } }

        protected override PrioTable GetPrioTable()
        {
            var prioTable = PrioTable.Left("!");
            prioTable += PrioTable.Left("property", "converter");
            prioTable = prioTable.Level
                (new[]
                 {
                     "++-",
                     "+?-",
                     "?--"
                 },
                 new[] {"(", "[", "{", "<frame>"},
                 new[] {")", "]", "}", "<end>"}
                );
            prioTable += PrioTable.Left("<common>");
            return prioTable;
        }

        protected override Dictionary<string, TokenClasses.TokenClass> GetTokenClasses()
        {
            return new Dictionary<string, TokenClasses.TokenClass>
                   {
                       {"converter", new Converter()},
                       {"property", new Property()}
                   };
        }

        protected override TokenClasses.TokenClass GetListClass() { throw new NotImplementedException(); }
        protected override TokenClasses.TokenClass GetRightParenthesisClass(int level) { throw new NotImplementedException(); }
        protected override TokenClasses.TokenClass GetLeftParenthesisClass(int level) { throw new NotImplementedException(); }
        protected override TokenClasses.TokenClass GetNumberClass() { throw new NotImplementedException(); }
        protected override TokenClasses.TokenClass GetNewTokenClass(string name) { throw new NotImplementedException(); }
    }
}