using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Parser;
using Reni.Proof.TokenClasses;

namespace Reni.Proof
{
    internal sealed class TokenFactory : Parser.TokenFactory<TokenClasses.TokenClass>
    {
        internal static TokenFactory Instance { get { return new TokenFactory(); } }

        protected override TokenClasses.TokenClass NewTokenClass(string name) { return new UserSymbol(); }

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

        protected override Dictionary<string, TokenClasses.TokenClass> GetTokenClasses()
        {
            var result =
                new Dictionary<string, TokenClasses.TokenClass>
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

        protected override TokenClasses.TokenClass GetListClass() { throw new NotImplementedException(); }
        protected override TokenClasses.TokenClass GetRightParenthesisClass(int level) { return new RightParenthesis(level); }
        protected override TokenClasses.TokenClass GetLeftParenthesisClass(int level) { return new LeftParenthesis(level); }
        protected override TokenClasses.TokenClass GetNumberClass() { return new Number(); }

        internal Minus Minus { get { return (Minus) TokenClass("-"); } }
        internal Equal Equal { get { return (Equal) TokenClass("="); } }
        internal Plus Plus { get { return (Plus) TokenClass("+"); } }
    }
}