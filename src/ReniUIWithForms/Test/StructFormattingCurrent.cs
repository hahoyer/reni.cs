using hw.UnitTest;
using NUnit.Framework;

namespace ReniUI.Test
{
    [UnitTest]
    [StructFormatting]
    public sealed class StructFormattingCurrent : DependenceProvider
    {
        [Test]
        [UnitTest]
        public void FlatList2()
        {
            const string text = @"aaaaa;ccccc";
            const string expectedText = @"aaaaa; ccccc";

            text.SimpleTest(expectedText );
        }

        [Test]
        [UnitTest]
        public void FlatList2Long()
        {
            const string text = @"aaaaa;ccccc";
            const string expectedText = @"aaaaa;
ccccc";

            text.SimpleTest(expectedText, 10 );
        }

    }
}