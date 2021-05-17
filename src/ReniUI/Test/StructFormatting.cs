using hw.DebugFormatter;
using hw.UnitTest;
using NUnit.Framework;
using ReniUI.Formatting;

namespace ReniUI.Test
{
    [UnitTest]
    [TestFixture]
    public sealed class StructFormatting : DependenceProvider
    {
        [Test]
        [UnitTest]
        public void One() => @"aaaaa".SimpleTest();

        [Test]
        [UnitTest]
        public void Two() => @"aaaaa bbbbb".SimpleTest();

        [Test]
        [UnitTest]
        public void BreakLine() => @"aaaaa bbbbb".SimpleTest(maxLineLength: 10, expected: @"aaaaa
    bbbbb");

        [Test]
        [UnitTest]
        public void BreakLine3() => @"aaaaa bbbbb ccccc".SimpleTest(maxLineLength: 10, expected: @"aaaaa
    bbbbb
    ccccc");

        [Test]
        [UnitTest]
        public void BreakLineWithLimit1() => @"aaaaa 

bbbbb".SimpleTest(@"aaaaa
    bbbbb",
            emptyLineLimit: 1);

        [Test]
        [UnitTest]
        public void BreakLineWithLimit0() => @"aaaaa 

bbbbb".SimpleTest(@"aaaaa bbbbb",
            emptyLineLimit: 0);


        [UnitTest]
        [Test]
        public void OmitSpaceWhenLineBreakRemains()
        {
            const string text =
                @"a
b";

            var expectedText = @"a
    b"
                .Replace("\r\n", "\n");

            text.SimpleTest(expectedText);
        }

        [UnitTest]
        [Test]
        public void UseSpaceWhenLineBreakIsRemoved()
        {
            const string text =
                @"a
b";

            var expectedText = @"a b".Replace("\r\n", "\n");

            var compiler = CompilerBrowser.FromText(text);
            var newSource = compiler.Reformat
                (
                    new ReniUI.Formatting.Configuration
                        {EmptyLineLimit = 0}.Create()
                )
                .Replace("\r\n", "\n");

            (newSource == expectedText).Assert("\n\"" + newSource + "\"");
        }
    }
}