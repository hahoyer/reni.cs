using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Parser;
using Reni.ReniParser.TokenClasses;

namespace Reni.ReniParser
{
    internal sealed class DeclarationTokenFactory : TokenFactory<TokenClass>
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

        protected override Dictionary<string, TokenClass> GetTokenClasses()
        {
            return new Dictionary<string, TokenClass>
                   {
                       {"converter", new Converter()},
                       {"property", new Property()}
                   };
        }

        protected override TokenClass GetListClass() { throw new NotImplementedException(); }
        protected override TokenClass RightParentethesisClass(int level) { throw new NotImplementedException(); }
        protected override TokenClass LeftParentethesisClass(int level) { throw new NotImplementedException(); }
        protected override TokenClass NewTokenClass(string name) { throw new NotImplementedException(); }
    }
}