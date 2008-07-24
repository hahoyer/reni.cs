using System;
using HWClassLibrary.Debug;
using NUnit.Framework;
using Reni.FeatureTest;
using Reni.FeatureTest.Structure;

namespace Reni.Parser
{
    [TestFixture]
    public class Test : CompilerTest
    {
        [SetUp]
        public new void Start()
        {
            base.Start();
        }

        public override void Run() { 
        }

        /// <summary>
        /// Special test, will not work automatically.
        /// </summary>
        /// created 18.07.2007 01:27 on HAHOYER-DELL by hh
        [Test, Explicit]
        public void SimpleFunction()
        {
            var syntaxPrototype = LikeSyntax.Expression(null, "f", LikeSyntax.Null);
            Parameters.Trace.Source = true;
            Parameters.ParseOnly = true;
            RunCompiler("SimpleFunction", @"f()", c => syntaxPrototype.AssertLike(c.Syntax));
        }

        [Test, Category(Worked)]
        public void Add2Numbers()
        {
            var syntaxPrototype =
                (LikeSyntax.Number(2) + LikeSyntax.Number(4)).dump_print;
            Parameters.ParseOnly = true;
            RunCompiler("Add2Numbers", @"(2+4) dump_print", c => syntaxPrototype.AssertLike(c.Syntax));
        }

        [Test, Category(Worked)]
        public void AlternativePrioTableProperty1()
        {
            var syntaxPrototype = LikeSyntax.Struct(
                new[] {LikeSyntax.Number(3)},
                new[] {LikeSyntax.Declaration("x", 0)},
                new int[] {},
                new[] {"x"}
                );
            Parameters.ParseOnly = true;
            RunCompiler("UseAlternativePrioTable", @"!property x: 3", c => syntaxPrototype.AssertLike(c.Syntax));
        }

        [Test, Category(Worked)]
        public void AlternativePrioTableProperty2()
        {
            var syntaxPrototype = LikeSyntax.Struct(
                new[] {LikeSyntax.Number(3)},
                new[] {LikeSyntax.Declaration("property", 0)},
                new int[] {},
                new[] {"property"}
                );
            Parameters.ParseOnly = true;
            RunCompiler("UseAlternativePrioTable", @"!property property: 3", c => syntaxPrototype.AssertLike(c.Syntax));
        }

        [Test, Category(Worked)]
        public void AlternativePrioTableConverter()
        {
            var syntaxPrototype = LikeSyntax.Struct(
                new[] {LikeSyntax.Number(3)},
                new Declaration[] {},
                new[] {0},
                new string[] {}
                );
            Parameters.ParseOnly = true;
            RunCompiler("UseAlternativePrioTable", @"!converter: 3", c => syntaxPrototype.AssertLike(c.Syntax));
        }

        [Test, Category(Worked)]
        public void AlternativePrioTableConverterAndProperty()
        {
            var syntaxPrototype = LikeSyntax.Struct(
                new[] {LikeSyntax.Number(3), LikeSyntax.Number(4)},
                new[] {LikeSyntax.Declaration("x", 1)},
                new[] {0},
                new[] {"x"}
                );
            Parameters.ParseOnly = true;
            RunCompiler("UseAlternativePrioTable", @"!converter: 3; !property x: 4",
                        c => syntaxPrototype.AssertLike(c.Syntax));
        }
    }
}