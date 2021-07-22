using System.Linq;
using hw.DebugFormatter;
using hw.UnitTest;
using NUnit.Framework;
using ReniUI.Formatting;

namespace ReniUI.Test
{
    [UnitTest]
    [TestFixture]
    [Formatting]
    public sealed class FormattingWithComments : DependenceProvider
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