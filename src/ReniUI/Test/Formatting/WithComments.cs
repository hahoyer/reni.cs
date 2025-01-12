using hw.UnitTest;

namespace ReniUI.Test.Formatting;

[UnitTest]

[Complex]
public sealed class WithComments : DependenceProvider
{
    
    [UnitTest]
    public void ReformatComments()
    {
        const string text =
            @"137;

################################################################
# Test
################################################################
                   3
";

        const string expectedText =
            @"137;
################################################################
# Test
################################################################
3
";

        text.SimpleFormattingTest(expectedText, 120, 1);
    }
}