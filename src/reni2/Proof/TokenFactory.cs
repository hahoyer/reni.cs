using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Parser;

namespace Reni.Proof
{
    internal sealed class TokenFactory : TokenFactory<TokenClass>
    {
        internal static ITokenFactory Instance { get { return new TokenFactory(); } }

        protected override TokenClass NewTokenClass(string name) { return new UserSymbol(); }

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

        protected override Dictionary<string, TokenClass> GetTokenClasses()
        {
            var result =
                new Dictionary<string, TokenClass>
                {
                    {"=", new Equal()},
                    {">", new CompareOperator()},
                    {">=", new CompareOperator()},
                    {"<", new CompareOperator()},
                    {"<=", new CompareOperator()},
                    {"<>", new NotEqual()},
                    {"-", new Sign()},
                    {"&", new And()},
                    {"!", new Exclamation()},
                    {"+", new Sign()},
                    {"/", new Slash()},
                    {"*", new Star()},
                    {"^", new Caret()},
                    {"Integer", new Integer()},
                    {"gcd", new GreatesCommonDenominator()},
                    {"elem", new Element()}
                };
            return result;
        }

        protected override TokenClass GetListClass() { throw new NotImplementedException(); }

        protected override TokenClass GetRightParenthesisClass(int level)
        {
            Tracer.Assert(level == 0);
            return new RightParenthesis();
        }

        protected override TokenClass GetLeftParenthesisClass(int level)
        {
            Tracer.Assert(level == 0);
            return new LeftParenthesis();
        }

        protected override TokenClass GetNumberClass() { return new Proof.Number(); }
    }

}