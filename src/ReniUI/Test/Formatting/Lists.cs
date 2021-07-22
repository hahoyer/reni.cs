using hw.DebugFormatter;
using hw.UnitTest;
using NUnit.Framework;
using ReniUI;
using ReniUI.Test;
using ReniUI.Test.Formatting;

namespace reniUI.Test.Formatting
{
    [UnitTest]
    [TestFixture]
    public sealed class Lists
        : DependenceProvider
    {
        [Test]
        [UnitTest]
        public void SimpleLine()
        {
            const string Text = @"(1,3,4,6)";
            var compiler = CompilerBrowser.FromText(Text);
            var span = compiler.Source.All;
            var trimmed = compiler.Reformat(targetPart: span);

            (trimmed == "(1, 3, 4, 6)").Assert(trimmed);
        }

        [Test]
        [UnitTest]
        public void SingleElementList()
        {
            const string text = @"((aaaaaddddd))";
            const string expectedText = @"((
    aaaaaddddd
))";

            text.SimpleFormattingTest(expectedText, 10, 1);
        }

        [Test]
        [UnitTest]
        public void SingleElementListFlat()
        {
            const string text = @"(aaaaaddddd)";
            const string expectedText = @"(aaaaaddddd)";

            text.SimpleFormattingTest(expectedText, 14, 1);
        }

        [Test]
        [UnitTest]
        public void StraightList()
        {
            const string text = @"(aaaaa;ccccc)";
            const string expectedText = @"(
    aaaaa;
    ccccc
)";
            text.SimpleFormattingTest(expectedText, 10, 1);
        }

        [Test]
        [UnitTest]
        public void StraightListWithMultipleBrackets()
        {
            const string text = @"((aaaaa;ccccc))";
            const string expectedText = @"((
    aaaaa;
    ccccc
))";
            text.SimpleFormattingTest(expectedText, 10, 1);
        }

        [Test]
        [UnitTest]
        public void FlatList2()
        {
            const string text = @"aaaaa;ccccc";
            const string expectedText = @"aaaaa; ccccc";

            text.SimpleFormattingTest(expectedText);
        }

        [Test]
        [UnitTest]
        public void ListEndsWithListToken()
        {
            const string text = @"(aaaaa;bbbbb;ccccc;)";
            const string expectedText = @"(
    aaaaa;
    bbbbb;
    ccccc;
)";

            text.SimpleFormattingTest(expectedText, 20, 1);
        }

        [Test]
        [UnitTest]
        public void ListSeparatorAtEnd()
        {
            const string text = @"aaaaa;";
            const string expectedText = @"aaaaa;";

            var compiler = CompilerBrowser.FromText(text);
            var newSource = compiler.FlatFormat(false);
            (newSource == expectedText).Assert("\n\"" + newSource + "\"");
        }

        [Test]
        [UnitTest]
        public void ListLineBreakTest2()
        {
            const string text = @"aaaaa;bbbbb";
            const string expectedText = @"aaaaa;
bbbbb";

            text.SimpleFormattingTest(expectedText, 10, 1);
        }

        [Test]
        [UnitTest]
        public void ListLineBreakTest3()
        {
            const string text = @"aaaaa;bbbbb;ccccc";
            const string expectedText = @"aaaaa;
bbbbb;
ccccc";

            text.SimpleFormattingTest(expectedText, 10, 1);
        }

        [Test]
        [UnitTest]
        public void ListTest1()
        {
            const string text = @"aaaaa";
            const string expectedText = @"aaaaa";

            text.SimpleFormattingTest(expectedText, 10, 1);
        }

        [Test]
        [UnitTest]
        public void ListTest2()
        {
            const string text = @"aaaaa;bbbbb";
            const string expectedText = @"aaaaa; bbbbb";

            text.SimpleFormattingTest(expectedText, 20, 1);
        }

        [Test]
        [UnitTest]
        public void MultilineBreakTest()
        {
            const string text =
                @"(ccccc,aaaaa bbbbb, )";

            var expectedText = @"(
    ccccc,

    aaaaa
        bbbbb,
)".Replace("\r\n", "\n");

            text.SimpleFormattingTest(expectedText, 10, 1);
        }

        [Test]
        [UnitTest]
        public void MultilineBreakTest1()
        {
            const string text =
                @"(ccccc,aaaaa bbbbb)";

            var expectedText = @"(
    ccccc,

    aaaaa
        bbbbb
)".Replace("\r\n", "\n");

            text.SimpleFormattingTest(expectedText, 10, 1);
        }

        [Test]
        [UnitTest]
        public void MultilineBreakTest11()
        {
            const string text =
                @"1;

repeat:
    @ ^ while()
    then
    (
        ^ body(),
        repeat(^)
    );

2";

            text.SimpleFormattingTest(null, 20, 0);
        }
    }
}