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
        const string text = @"nnnn!public: vvvv";

        text.SimpleFormattingTest();
    }

    [Test]
    [UnitTest]
    public void OneSpace()
    {
        const string text = @"nnnn! public : vvvv";
        const string expectedText = @"nnnn!public: vvvv";

        text.SimpleFormattingTest(expectedText);
    }
    [Test]
    [UnitTest]
    public void MultipleTags()
    {
        const string text = @"nnnn ! (  mutable,     public      ): vvvv";
        const string expectedText = @"nnnn!(mutable, public): vvvv";

        text.SimpleFormattingTest(expectedText);
    }
}