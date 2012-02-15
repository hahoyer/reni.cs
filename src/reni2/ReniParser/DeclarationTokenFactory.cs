// 
//     Project Reni2
//     Copyright (C) 2011 - 2012 Harald Hoyer
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//     
//     Comments, bugs and suggestions to hahoyer at yahoo.de

using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Parser;
using Reni.TokenClasses;

namespace Reni.ReniParser
{
    sealed class DeclarationTokenFactory : TokenFactory<TokenClasses.TokenClass>
    {
        internal static DeclarationTokenFactory Instance { get { return new DeclarationTokenFactory(); } }

        protected override PrioTable GetPrioTable()
        {
            var prioTable = PrioTable.Left("!");
            prioTable += PrioTable.Left("converter");
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

        protected override DictionaryEx<string, TokenClasses.TokenClass> GetTokenClasses()
        {
            return new DictionaryEx<string, TokenClasses.TokenClass>
                   {
                       {"converter", new ConverterToken()},
                   };
        }

        protected override TokenClasses.TokenClass GetSyntaxError(string message) { return new SyntaxError(message); }
    }
}