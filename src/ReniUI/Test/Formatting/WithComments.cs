using hw.UnitTest;
using NUnit.Framework;

namespace ReniUI.Test.Formatting
{
    [UnitTest]
    [TestFixture]
    [Complex]
    public sealed class WithComments : DependenceProvider
    {

        [Test]
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
}