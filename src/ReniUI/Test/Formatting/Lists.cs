using hw.UnitTest;

namespace ReniUI.Test.Formatting;

[UnitTest]

[FlatStrings]
public sealed class Lists : DependenceProvider
{
    
    [UnitTest]
    public void SimpleLine()
    {
        const string text = @"(1,3,4,6)";
        var compiler = CompilerBrowser.FromText(text);
        var span = compiler.Source.All;
        var trimmed = compiler.Reformat(targetPart: span);

        (trimmed == "(1, 3, 4, 6)").Assert(trimmed);
    }

    
    [UnitTest]
    public void SingleElementList()
    {
        const string text = @"((aaaaaddddd))";
        const string expectedText = @"((
  aaaaaddddd
))";

        text.SimpleFormattingTest(expectedText, 10, 1);
    }

    
    [UnitTest]
    public void SingleElementListFlat()
    {
        const string text = @"(aaaaaddddd)";
        const string expectedText = @"(aaaaaddddd)";

        text.SimpleFormattingTest(expectedText, 14, 1);
    }

    
    [UnitTest]
    public void StraightList()
    {
        const string text = @"(aaaaa;ccccc)";
        const string expectedText = @"(
  aaaaa;
  ccccc
)";
        text.SimpleFormattingTest(expectedText, 10, 1);
    }

    
    [UnitTest]
    public void StraightList3()
    {
        const string text = @"(aaaaa;bbbbb;ccccc)";
        const string expectedText = @"(
  aaaaa;
  bbbbb;
  ccccc
)";
        text.SimpleFormattingTest(expectedText, 10, 1);
    }

    
    [UnitTest]
    public void StraightListWithMultipleBrackets()
    {
        const string text = @"((aaaaa;ccccc))";
        const string expectedText = @"((
  aaaaa;
  ccccc
))";
        text.SimpleFormattingTest(expectedText, 10, 1);
    }

    
    [UnitTest]
    public void FlatList2()
    {
        const string text = @"aaaaa;ccccc";
        const string expectedText = @"aaaaa; ccccc";

        text.SimpleFormattingTest(expectedText);
    }

    
    [UnitTest]
    public void ListEndsWithListToken()
    {
        const string text = @"(aaaaa;bbbbb;ccccc;)";
        const string expectedText = @"(
  aaaaa;
  bbbbb;
  ccccc;
)";

        text.SimpleFormattingTest(expectedText, 19, 1);
    }

    
    [UnitTest]
    public void ListSeparatorAtEnd()
    {
        const string text = @"aaaaa;";
        const string expectedText = @"aaaaa;";

        var compiler = CompilerBrowser.FromText(text);
        var newSource = compiler.FlatFormat(false);
        (newSource == expectedText).Assert("\n\"" + newSource + "\"");
    }

    
    [UnitTest]
    public void ListLineBreakTest2()
    {
        const string text = @"aaaaa;bbbbb";
        const string expectedText = @"aaaaa;
bbbbb";

        text.SimpleFormattingTest(expectedText, 10, 1);
    }

    
    [UnitTest]
    public void ListLineBreakTest3()
    {
        const string text = @"aaaaa;bbbbb;ccccc";
        const string expectedText = @"aaaaa;
bbbbb;
ccccc";

        text.SimpleFormattingTest(expectedText, 10, 1);
    }

    
    [UnitTest]
    public void ListTest1()
    {
        const string text = @"aaaaa";
        const string expectedText = @"aaaaa";

        text.SimpleFormattingTest(expectedText, 10, 1);
    }

    
    [UnitTest]
    public void ListTest2()
    {
        const string text = @"aaaaa;bbbbb";
        const string expectedText = @"aaaaa; bbbbb";

        text.SimpleFormattingTest(expectedText, 20, 1);
    }

    
    [UnitTest]
    public void MultilineBreakTest()
    {
        const string text =
            @"(ccccc,aaaaa bbbbb, )";

        var expectedText = @"(
  ccccc,

  aaaaa
  bbbbb,
)".Replace("\r\n", "\n");

        text.SimpleFormattingTest(expectedText, 10, 1);
    }

    
    [UnitTest]
    public void MultilineBreakTest1()
    {
        const string text =
            @"(ccccc,aaaaa bbbbb)";

        var expectedText = @"(
  ccccc,

  aaaaa
  bbbbb
)".Replace("\r\n", "\n");

        text.SimpleFormattingTest(expectedText, 10, 1);
    }

    [UnitTest]
    public void ListOfLists()
    {
        // ReSharper disable once StringLiteralTypo
        const string text
            = "Auswahl: @{  Ja: (Type: ^^, Value: \"Ja\"),  " +
            "Nein: (Type: ^^, Value: \"Nein\"), " +
            "Vielleicht: (Type: ^^, Value: \"Vielleicht\")}";
        const string expected = @"Auswahl: @
{
  Ja: (Type: ^^, Value: ""Ja""),
  Nein: (Type: ^^, Value: ""Nein""),
  Vielleicht: (Type: ^^, Value: ""Vielleicht"")
}";
        text.SimpleFormattingTest(expected, indentCount: 2, lineBreaksAtComplexDeclaration: true);
    }
}