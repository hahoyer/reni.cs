using hw.UnitTest;

namespace ReniUI.Test.Formatting;

[UnitTest]
[Declaration]
public sealed class Exclamation : DependenceProvider
{
    [UnitTest]
    public void Simple()
    {
        const string text = @"nnnn!public: vvvv";

        text.SimpleFormattingTest();
    }

    [UnitTest]
    public void OneSpace()
    {
        const string text = @"nnnn! public : vvvv";
        const string expectedText = @"nnnn!public: vvvv";

        text.SimpleFormattingTest(expectedText);
    }
    [UnitTest]
    public void MultipleTags()
    {
        const string text = @"nnnn ! (  mutable,     public      ): vvvv";
        const string expectedText = @"nnnn!(mutable, public): vvvv";

        text.SimpleFormattingTest(expectedText);
    }
}