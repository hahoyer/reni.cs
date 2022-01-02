using hw.UnitTest;

namespace ReniUI.Test.Formatting;

[UnitTest]
[TestFixture]
[Declaration]
public sealed class Exclamation : DependenceProvider
{
    [Test]
    [UnitTest]
    public void Simple()
    {
        const string text = @"!public nnnn: vvvv";

        text.SimpleFormattingTest();
    }

    [Test]
    [UnitTest]
    public void OneSpace()
    {
        const string text = @"! public nnnn: vvvv";
        const string expectedText = @"!public nnnn: vvvv";

        text.SimpleFormattingTest(expectedText);
    }
    [Test]
    [UnitTest]
    public void MultipleTags()
    {
        const string text = @"! (  mutable     public      )nnnn: vvvv";
        const string expectedText = @"!(mutable public) nnnn: vvvv";

        text.SimpleFormattingTest(expectedText);
    }
}