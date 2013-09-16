#region Copyright (C) 2013

//     Project Reni2
//     Copyright (C) 2011 - 2013 Harald Hoyer
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

#endregion

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
        public DeclarationTokenFactory()
            : base(PrioTable) { }
        internal static DeclarationTokenFactory Instance { get { return new DeclarationTokenFactory(); } }

        static PrioTable PrioTable
        {
            get
            {
                var prioTable = PrioTable.Left("!");
                prioTable += PrioTable.Left("converter");
                prioTable = prioTable.ParenthesisLevel
                    (
                        new[] {"(", "[", "{", PrioTable.BeginOfText},
                        new[] {")", "]", "}", PrioTable.EndOfText}
                    );
                prioTable += PrioTable.Left(PrioTable.Any);
                return prioTable;
            }
        }

        protected override FunctionCache<string, TokenClass> GetPredefinedTokenClasses()
        {
            return new FunctionCache<string, TokenClass>
            {
                {"converter", new ConverterToken()},
            };
        }

        protected override TokenClass GetSyntaxError(string message) { return new SyntaxError(message); }
    }
}