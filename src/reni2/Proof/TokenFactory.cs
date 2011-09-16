//     Compiler for programming language "Reni"
//     Copyright (C) 2011 Harald Hoyer
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

using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.Helper;
using Reni.Parser;
using Reni.Proof.TokenClasses;

namespace Reni.Proof
{
    internal sealed class TokenFactory : Parser.TokenFactory<TokenClasses.TokenClass>
    {
        internal static TokenFactory Instance { get { return new TokenFactory(); } }

        protected override TokenClasses.TokenClass GetNewTokenClass(string name) { return new UserSymbol(); }

        protected override PrioTable GetPrioTable()
        {
            var x = PrioTable.Left("<common>");

            x += PrioTable.Right("^");
            x += PrioTable.Left("*", "/", "\\", "gcd");
            x += PrioTable.Left("+", "-");

            x += PrioTable.Left("<", ">", "<=", ">=");
            x += PrioTable.Left("=", "<>");

            x += PrioTable.Left("elem");
            x += PrioTable.Left("&");
            x += PrioTable.Left("|");

            x += PrioTable.Right(",");
            x += PrioTable.Right(";");

            x = x.Level
                (new[]
                 {
                     "++-",
                     "+?-",
                     "?--"
                 },
                 new[] {"(", "[", "{", "<frame>"},
                 new[] {")", "]", "}", "<end>"}
                );
            //Tracer.FlaggedLine("\n"+x+"\n");
            return x;
        }

        protected override DictionaryEx<string, TokenClasses.TokenClass> GetTokenClasses()
        {
            var result =
                new DictionaryEx<string, TokenClasses.TokenClass>
                {
                    {"=", new Equal()},
                    {"-", new Minus()},
                    {"&", new And()},
                    {"+", new Plus()},
                    {"^", new Caret()},
                    {"Integer", new Integer()},
                    {"gcd", new GreatesCommonDenominator()},
                    {"elem", new Element()}
                };
            return result;
        }

        protected override TokenClasses.TokenClass GetListClass() { return new List(); }
        protected override TokenClasses.TokenClass GetRightParenthesisClass(int level) { return new RightParenthesis(level); }
        protected override TokenClasses.TokenClass GetLeftParenthesisClass(int level) { return new LeftParenthesis(level); }
        protected override TokenClasses.TokenClass GetNumberClass() { return new TokenClasses.Number(); }

        internal Minus Minus { get { return (Minus) TokenClass("-"); } }
        internal Equal Equal { get { return (Equal) TokenClass("="); } }
        internal Plus Plus { get { return (Plus) TokenClass("+"); } }

        protected override TokenClasses.TokenClass GetSyntaxError(string message) { return new SyntaxError(message); }

        private sealed class SyntaxError : TokenClasses.TokenClass
        {
            private readonly string _message;
            public SyntaxError(string message) { _message = message; }
        }
    }
}