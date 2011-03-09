using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Feature;
using Reni.Parser;
using Reni.Sequence;

namespace Reni.Proof
{
    public static class Main
    {
        public static void Run()
        {
            var statement = new Holder(@"
a € Integer & 
b € Integer & 
c € Integer & 
a^2 + b^2 = c^2 &
a ggt b = 1
");
            statement.Replace("a", "x+y");
        }
    }

    internal sealed class Holder
    {
        private static readonly ParserInst _parser = new ParserInst(new Scanner(), TokenFactory.Instance);
        private readonly string _text;

        public Holder(string text) { _text = text; }

        public void Replace(string target, string value) { throw new NotImplementedException(); }
    }

    internal abstract class TokenClass : Parser.TokenClasses.TokenClass
    {}

    internal sealed class TokenFactory : TokenFactory<TokenClass>
    {
        internal static ITokenFactory Instance { get { return new TokenFactory(); } }

        protected override TokenClass NewTokenClass(string name) { throw new NotImplementedException(); }

        protected override PrioTable GetPrioTable()
        {
            var x = PrioTable.Left("<common>");

            x += PrioTable.Right("^");
            x += PrioTable.Left("*", "/", "\\", "ggt", "kgv");
            x += PrioTable.Left("+", "-");

            x += PrioTable.Left("<", ">", "<=", ">=");
            x += PrioTable.Left("=", "<>");

            x += PrioTable.Left("€");
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
                    {"!", new Exclamation()},
                    {"+", new Sign()},
                    {"/", new Slash()},
                    {"*", new Star()},
                    {"^", new Caret()},
                    {"€", new EuroSign()},
                    {"ggt", new GGT()},
                    {"kgv", new KGV()},
                    {"Integer", new Integer()},
                    {"type", new TypeOperator()}
                };
            return result;
        }

        protected override TokenClass GetListClass() { throw new NotImplementedException(); }
        protected override TokenClass RightParentethesisClass(int level) { throw new NotImplementedException(); }
        protected override TokenClass LeftParentethesisClass(int level) { throw new NotImplementedException(); }
    }

    internal class EuroSign : TokenClass
    {}

    internal class TypeOperator : TokenClass
    {}

    internal class Integer : TokenClass
    {}

    internal class KGV : TokenClass
    {}

    internal class GGT : TokenClass
    {}

    internal class Caret : TokenClass
    {}

    internal class Star : TokenClass
    {}

    internal class Slash : TokenClass
    {}

    internal class Sign : TokenClass
    {}

    internal class Exclamation : TokenClass
    {}

    internal class NotEqual : TokenClass
    {}

    internal class CompareOperator : TokenClass
    {}

    internal class Equal : TokenClass
    {}
}