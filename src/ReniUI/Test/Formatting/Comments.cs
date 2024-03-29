using hw.UnitTest;

namespace ReniUI.Test.Formatting;

[UnitTest]
[TestFixture]
public sealed class Comments : DependenceProvider
{
    [UnitTest]
    [Test]
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
    [Test]
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
    [Test]
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
    [Test]
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
    [Test]
    public void SeparatorBeforeComment1()
    {
        const string text =
            @"head#( asdf )#";
        const string expectedText =
            @"head #( asdf )#";
        text.SimpleFormattingTest(expectedText, emptyLineLimit: 0);
    }

    [UnitTest]
    [Test]
    public void SeparatorBeforeComment2()
    {
        const string text =
            @"head #( asdf )#";
        const string expectedText =
            @"head #( asdf )#";
        text.SimpleFormattingTest(expectedText, emptyLineLimit: 0);
    }

    [UnitTest]
    [Test]
    public void SeparatorBeforeComment3()
    {
        const string text =
            @"head     #( asdf )#";
        const string expectedText =
            @"head #( asdf )#";
        text.SimpleFormattingTest(expectedText, emptyLineLimit: 0);
    }

    [UnitTest]
    [Test]
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
    [Test]
    public void IndentWithLineComment()
    {
        const string text = @"(
  cargo, #
  12
)";
        text.SimpleFormattingTest();
    }
}