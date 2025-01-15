using hw.UnitTest;
using Reni.FeatureTest.Helper;

namespace Reni.ParserTest;

[UnitTest]
[ParserTest]
public sealed class AlternativePrioTableTest : CompilerTest
{
    [UnitTest]
    public void Converter()
    {
        var syntaxPrototype = LikeSyntax.Compound
        (
            [LikeSyntax.Number(3)],
            [],
            [0]
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
            [LikeSyntax.Number(3), LikeSyntax.Number(4)],
            [],
            [0, 1]
        );
        Parameters.CompilationLevel= CompilationLevel.Syntax;
        CreateFileAndRunCompiler
        (
            "UseAlternativePrioTable",
            @"!converter: 3; !converter: 4",
            expectedResult: c => c.AssertSyntaxIsLike(syntaxPrototype));
    }

    [UnitTest]
    public void KeyWordAsUserObject()
    {
        var syntaxPrototype = LikeSyntax.Compound
        (
            [LikeSyntax.Number(3)],
            [new Declaration("converter", 0)],
            []
        );
        Parameters.CompilationLevel= CompilationLevel.Syntax;
        CreateFileAndRunCompiler
        (
            "KeyWordAsUserObject",
            @"converter: 3",
            expectedResult: c => c.AssertSyntaxIsLike(syntaxPrototype));
    }
}