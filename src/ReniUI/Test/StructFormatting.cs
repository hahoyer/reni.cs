using hw.DebugFormatter;
using hw.UnitTest;
using NUnit.Framework;
using ReniUI.Formatting;
using Configuration = ReniUI.Formatting.Configuration;

namespace ReniUI.Test
{
    [UnitTest]
    [TestFixture]
    public sealed class StructFormatting : DependenceProvider
    {
        [Test]
        [UnitTest]
        public void BreakLine()
        {
            @"aaaaa bbbbb".SimpleFormattingTest(maxLineLength: 10, expected: @"aaaaa
    bbbbb");
        }

        [Test]
        [UnitTest]
        public void One()
        {
            @"aaaaa".SimpleFormattingTest();
        }

        [Test]
        [UnitTest]
        public void Two()
        {
            @"aaaaa bbbbb".SimpleFormattingTest();
        }

        [Test]
        [UnitTest]
        public void BreakLine3()
        {
            @"aaaaa bbbbb ccccc".SimpleFormattingTest(maxLineLength: 10, expected: @"aaaaa
    bbbbb
    ccccc");
        }

        [Test]
        [UnitTest]
        public void BreakLineWithLimit1()
        {
            @"aaaaa 

bbbbb".SimpleFormattingTest(@"aaaaa
    bbbbb",
                emptyLineLimit: 1);
        }

        [Test]
        [UnitTest]
        public void BreakLineWithLimit0()
        {
            @"aaaaa 

bbbbb".SimpleFormattingTest(@"aaaaa bbbbb",
                emptyLineLimit: 0);
        }


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

            text.SimpleFormattingTest(expectedText);
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
                    new Configuration
                        {EmptyLineLimit = 0}.Create()
                )
                .Replace("\r\n", "\n");


            (newSource == expectedText).Assert("\n\"" + newSource + "\"");
        }

        [UnitTest]
        [Test]
        public void EmptyBrackets()
        {
            const string text =
                @"a()
b";

            var expectedText = @"a() b".Replace("\r\n", "\n");

            var compiler = CompilerBrowser.FromText(text);
            var newSource = compiler.Reformat
                (
                    new Configuration
                        {EmptyLineLimit = 0}.Create()
                )
                .Replace("\r\n", "\n");

            (newSource == expectedText).Assert("\n\"" + newSource + "\"");
        }

        [UnitTest]
        public void Reformat()
        {
             const string text = @"repeat:
        @ ^ while()
        then
        (
            ^ body(),
            repeat(^)
        );
";
            const string expectedText = @"repeat:
    @ ^ while()
    then
    (
        ^ body(),
        repeat(^)
    );";

            text.SimpleFormattingTest(expectedText, 10, 0);
        }
    }
}