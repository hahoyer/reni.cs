using hw.UnitTest;
using NUnit.Framework;
using Reni.FeatureTest.Helper;

namespace Reni.ParserTest
{
    [UnitTest]
    [ParserTest]
    [TestFixture]
    public sealed class AlternativePrioTableTest : CompilerTest
    {
        [UnitTest]
        public void Converter()
        {
            var syntaxPrototype = LikeSyntax.Compound
            (
                new[] {LikeSyntax.Number(3)},
                new Declaration[] { },
                new[] {0}
            );
            Parameters.CompilationLevel= CompilationLevel.Syntax;
            CreateFileAndRunCompiler
            (
                "UseAlternativePrioTable",
                @"!converter: 3",
                expectedResult: c => c.AssertSyntaxIsLike(syntaxPrototype));
        }

        [UnitTest]
        public void ConverterAndProperty()
        {
            var syntaxPrototype = LikeSyntax.Compound
            (
                new[] {LikeSyntax.Number(3), LikeSyntax.Number(4)},
                new Declaration[] { },
                new[] {0, 1}
            );
            Parameters.CompilationLevel= CompilationLevel.Syntax;
            CreateFileAndRunCompiler
            (
                "UseAlternativePrioTable",
                @"!converter: 3; !converter: 4",
                expectedResult: c => c.AssertSyntaxIsLike(syntaxPrototype));
        }

        [UnitTest]
        [Test]
        public void KeyWordAsUserObject()
        {
            var syntaxPrototype = LikeSyntax.Compound
            (
                new[] {LikeSyntax.Number(3)},
                new[] {new Declaration("converter", 0)},
                new int[] { }
            );
            Parameters.CompilationLevel= CompilationLevel.Syntax;
            CreateFileAndRunCompiler
            (
                "KeyWordAsUserObject",
                @"converter: 3",
                expectedResult: c => c.AssertSyntaxIsLike(syntaxPrototype));
        }
    }
}