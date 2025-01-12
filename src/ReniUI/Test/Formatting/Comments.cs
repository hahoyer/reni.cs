using hw.UnitTest;

namespace ReniUI.Test.Formatting;

[UnitTest]
public sealed class Comments : DependenceProvider
{
    [UnitTest]
    public void SingleLine()
    {
        var text =
            @"#asdf1_
text1
";
        var expectedText =
            @"#asdf1_
text1
";

        text.SimpleFormattingTest(expectedText, spaceReplacement: '_');
    }

    [UnitTest]
    public void SingleLineWithLineBreak()
    {
        var text =
            @"#asdf

texxxxxxxxxxxt
";
        var expectedText =
            @"#asdf

texxxxxxxxxxxt
";

        text.SimpleFormattingTest(expectedText);
    }

    [UnitTest]
    public void SingleLineWithVolatileLineBreak()
    {
        var text =
            @"#asdf

texxxxxxxxxxxt
";
        var expectedText =
            @"#asdf
texxxxxxxxxxxt";

        text.SimpleFormattingTest(expectedText, emptyLineLimit: 0);
    }

    [UnitTest]
    public void MultiLineCommentSingleLine()
    {
        const string text =
            @"#( asdf )#  texxxxxxxxxxxt
";
        const string expectedText =
            @"#( asdf )# texxxxxxxxxxxt
";
        text.SimpleFormattingTest(expectedText);
    }

    [UnitTest]
    public void SeparatorBeforeComment1()
    {
        const string text =
            @"head#( asdf )#";
        const string expectedText =
            @"head #( asdf )#";
        text.SimpleFormattingTest(expectedText, emptyLineLimit: 0);
    }

    [UnitTest]
    public void SeparatorBeforeComment2()
    {
        const string text =
            @"head #( asdf )#";
        const string expectedText =
            @"head #( asdf )#";
        text.SimpleFormattingTest(expectedText, emptyLineLimit: 0);
    }

    [UnitTest]
    public void SeparatorBeforeComment3()
    {
        const string text =
            @"head     #( asdf )#";
        const string expectedText =
            @"head #( asdf )#";
        text.SimpleFormattingTest(expectedText, emptyLineLimit: 0);
    }

    [UnitTest]
    public void SeparatorBeforeComment4()
    {
        const string text =
            @"head
#( asdf )#";
        const string expectedText =
            @"head #( asdf )#";
        text.SimpleFormattingTest(expectedText, emptyLineLimit: 0);
    }


    [UnitTest]
    public void IndentWithLineComment()
    {
        const string text = @"(
  cargo, #
  12
)";
        text.SimpleFormattingTest();
    }
}