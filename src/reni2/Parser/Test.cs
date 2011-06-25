using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.UnitTest;
using Reni.FeatureTest;
using Reni.FeatureTest.Structure;

namespace Reni.Parser
{
    [TestFixture]
    public sealed class ParserTest : CompilerTest
    {
        public override void Run() { }

        //[Test]
        public void SimpleFunction()
        {
            var syntaxPrototype = LikeSyntax.Expression(null, "f", LikeSyntax.Null);
            Parameters.Trace.Source = true;
            Parameters.ParseOnly = true;
            CreateFileAndRunCompiler("SimpleFunction", @"f()", c => syntaxPrototype.AssertLike(c.Syntax));
        }

        [Test]
        public void Add2Numbers()
        {
            var syntaxPrototype =
                (LikeSyntax.Number(2) + LikeSyntax.Number(4)).dump_print;
            Parameters.ParseOnly = true;
            CreateFileAndRunCompiler("Add2Numbers", @"(2+4) dump_print", c => syntaxPrototype.AssertLike(c.Syntax));
        }

        [Test]
        public void AlternativePrioTableProperty1()
        {
            var syntaxPrototype = LikeSyntax.Struct(
                new[] {LikeSyntax.Number(3)},
                new[] {LikeSyntax.Declaration("x", 0)},
                new int[] {},
                new[] {0}
                );
            Parameters.ParseOnly = true;
            CreateFileAndRunCompiler("UseAlternativePrioTable", @"!property x: 3", c => syntaxPrototype.AssertLike(c.Syntax));
        }

        [Test]
        public void AlternativePrioTableProperty2()
        {
            var syntaxPrototype = LikeSyntax.Struct(
                new[] {LikeSyntax.Number(3)},
                new[] {LikeSyntax.Declaration("property", 0)},
                new int[] {},
                new[] {0}
                );
            Parameters.ParseOnly = true;
            CreateFileAndRunCompiler("UseAlternativePrioTable", @"!property property: 3", c => syntaxPrototype.AssertLike(c.Syntax));
        }

        [Test]
        public void AlternativePrioTableConverter()
        {
            var syntaxPrototype = LikeSyntax.Struct(
                new[] {LikeSyntax.Number(3)},
                new Declaration[] {},
                new[] {0},
                new int[] {}
                );
            Parameters.ParseOnly = true;
            CreateFileAndRunCompiler("UseAlternativePrioTable", @"!converter: 3", c => syntaxPrototype.AssertLike(c.Syntax));
        }

        [Test]
        public void AlternativePrioTableConverterAndProperty()
        {
            var syntaxPrototype = LikeSyntax.Struct(
                new[] {LikeSyntax.Number(3), LikeSyntax.Number(4)},
                new[] {LikeSyntax.Declaration("x", 1)},
                new[] {0},
                new[] {1}
                );
            Parameters.ParseOnly = true;
            CreateFileAndRunCompiler("UseAlternativePrioTable", @"!converter: 3; !property x: 4",
                                     c => syntaxPrototype.AssertLike(c.Syntax));
        }
    }
}