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
        public void Start() => @"aaaaa 

bbbbb".SimpleTest(@"aaaaa
    bbbbb",
            10, 1);

        [Test]
        [UnitTest]
        public void LabeledEntriesInList()
        {
            const string Text =
                @"{aaaaa, label: bbbbb
    }";

            var expectedText =
                @"{
    aaaaa,

    label:
        bbbbb
}";
            Text.SimpleTest(expectedText, 10, 0);
        }
    }
}