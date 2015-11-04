using System;
using System.Collections.Generic;
using System.Linq;
using hw.UnitTest;
using NUnit.Framework;
using Reni.FeatureTest.Helper;

namespace Reni.ParserTest
{
    [UnitTest]
    [TestFixture]
    [ParserTest]
    public sealed class AlternativePrioTableTest : CompilerTest
    {
        [UnitTest, Test]
        public void Converter()
        {
            var syntaxPrototype = LikeSyntax.Compound
                (
                    new[] {LikeSyntax.Number(3)},
                    new Declaration[] {},
                    new[] {0}
                );
            Parameters.ParseOnly = true;
            CreateFileAndRunCompiler
                (
                    "UseAlternativePrioTable",
                    @"!converter: 3",
                    expectedResult: c => syntaxPrototype.AssertLike(c.Syntax));
        }

        [UnitTest]
        public void ConverterAndProperty()
        {
            var syntaxPrototype = LikeSyntax.Compound
                (
                    new[] {LikeSyntax.Number(3), LikeSyntax.Number(4)},
                    new Declaration[] {},
                    new[] {0, 1}
                );
            Parameters.ParseOnly = true;
            CreateFileAndRunCompiler
                (
                    "UseAlternativePrioTable",
                    @"!converter: 3; !converter: 4",
                    expectedResult: c => syntaxPrototype.AssertLike(c.Syntax));
        }
    }
}