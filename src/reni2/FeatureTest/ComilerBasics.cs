using System;
using HWClassLibrary.Debug;
using NUnit.Framework;
using Reni.Parser;
using Reni.Parser.TokenClass;
using Reni.Syntax;

namespace Reni.FeatureTest
{
    /// <summary>
    /// Operations on bitarrays
    /// </summary>
    [TestFixture]
    public class ComilerBasics : CompilerTest
    {
        [Test, Category(Worked)]
        public void Add2Numbers()
        {
            var syntaxPrototype = s.Expression(s.Expression(s.Number(2), "+", s.Number(4)), "dump_print");
            Parameters.ParseOnly = true;
            RunCompiler("Add2Numbers", @"(2+4) dump_print", c => syntaxPrototype.AssertLike(c.Syntax));
        }
        [Test, Category(UnderConstruction)]
        public void UseAlternativePrioTable()
        {
            var syntaxPrototype = s.Expression(s.Expression(s.Number(2), "+", s.Number(4)), "dump_print");
            Parameters.ParseOnly = true;
            RunCompiler("UseAlternativePrioTable", @"!property x: 3", c => syntaxPrototype.AssertLike(c.Syntax));
        }
    }

    internal abstract class s
    {
        public static s Number(int i)
        {
            return new sNumber(i);
        }

        private class sNumber : s
        {
            private readonly Int64 _i;

            internal sNumber(Int64 i)
            {
                _i = i;
            }

            public override void AssertLike(IParsedSyntax syntax)
            {
                var terminalSyntax = (TerminalSyntax) syntax;
                Tracer.Assert(terminalSyntax.Terminal is Number);
                Tracer.Assert(Parser.TokenClass.Number.ToInt64(terminalSyntax.Token) == _i);
            }
        }

        public static s Expression(s s1, string s2, s s3)
        {
            return new sExpression(s1, s2, s3);
        }

        public static s Expression(s s1, string s2)
        {
            return new sExpression(s1, s2, null);
        }

        internal class sExpression : s
        {
            private readonly s _s1;
            private readonly string _s2;
            private readonly s _s3;

            public sExpression(s s1, string s2, s s3)
            {
                _s1 = s1;
                _s2 = s2;
                _s3 = s3;
            }

            public override void AssertLike(IParsedSyntax syntax)
            {
                var ex = (ExpressionSyntax) syntax;
                AssertLike(_s1, ex.Left);
                Tracer.Assert(ex.Token.Name == _s2);
                AssertLike(_s3, ex.Right);
            }

            private static void AssertLike(s s3, ICompileSyntax right)
            {
                if(s3 == null)
                    Tracer.Assert(right == null);
                else
                    s3.AssertLike((IParsedSyntax) right);
            }
        }

        public abstract void AssertLike(IParsedSyntax syntax);
    }
}