#region Copyright (C) 2012

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

#endregion

using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.Helper;
using Reni.Parser;
using Reni.Proof.TokenClasses;

namespace Reni.Proof
{
    sealed class TokenFactory : Parser.TokenFactory<TokenClasses.TokenClass>
    {
        TokenFactory()
            : base(PrioTable) { }
       
        internal static TokenFactory Instance { get { return new TokenFactory(); } }

        protected override TokenClasses.TokenClass GetNewTokenClass(string name) { return new UserSymbol(); }

        static PrioTable PrioTable
        {
            get
            {
                var x = PrioTable.Left(PrioTable.Any);

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

                x = x.ParenthesisLevel
                    (
                        new[] {"(", "[", "{", PrioTable.BeginOfText},
                        new[] {")", "]", "}", PrioTable.EndOfText}
                    );
                //Tracer.FlaggedLine("\n"+x+"\n");
                return x;
            }
        }

        protected override DictionaryEx<string, TokenClasses.TokenClass> GetTokenClasses()
        {
            var result =
                new DictionaryEx<string, TokenClasses.TokenClass>
                {
                    {"{", new LeftParenthesis(1)},
                    {"[", new LeftParenthesis(2)},
                    {"(", new LeftParenthesis(3)},
                    {"}", new RightParenthesis(1)},
                    {"]", new RightParenthesis(2)},
                    {")", new RightParenthesis(3)},
                    {",", new List()},
                    {";", new List()},
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
        protected override TokenClasses.TokenClass GetEndOfTextClass() { return new RightParenthesis(0); }
        protected override TokenClasses.TokenClass GetBeginOfTextClass() { return new LeftParenthesis(0); }
        protected override TokenClasses.TokenClass GetNumberClass() { return new TokenClasses.Number(); }

        internal Minus Minus { get { return (Minus) TokenClass("-"); } }
        internal Equal Equal { get { return (Equal) TokenClass("="); } }
        internal Plus Plus { get { return (Plus) TokenClass("+"); } }

        protected override TokenClasses.TokenClass GetSyntaxError(string message) { return new SyntaxError(message); }

        sealed class SyntaxError : TokenClasses.TokenClass
        {
            readonly string _message;
            public SyntaxError(string message) { _message = message; }
        }
    }
}