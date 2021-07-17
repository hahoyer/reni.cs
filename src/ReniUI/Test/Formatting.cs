using System.Linq;
using hw.DebugFormatter;
using hw.UnitTest;
using NUnit.Framework;
using ReniUI.Formatting;

namespace ReniUI.Test
{
    [UnitTest]
    [TestFixture]
    [DeclarationFormatting]
    [StructFormatting]
    [ListFormatting]
    [ThenElseFormatting]
    public sealed class Formatting : DependenceProvider
    {
        [Test]
        [UnitTest]
        public void Reformat()
        {
            const string text =
                @"systemdata:{1 type instance(); Memory:((0 type *('100' to_number_of_base 64)) mutable) instance(); !mutable FreePointer: Memory array_reference mutable; repeat: @ ^ while() then
    (
        ^ body(),
        repeat(^)
    );}; 1 = 1 then 2 else 4; 3; (Text('H') << 'allo') dump_print ";

            var expectedText =
                @"systemdata:
{
    1 type instance();
    Memory: ((0 type *('100' to_number_of_base 64)) mutable) instance();
    !mutable FreePointer: Memory array_reference mutable;
    repeat: @ ^ while() then(^ body(), repeat(^));
};

1 = 1 then 2 else 4;
3;
(Text('H') << 'allo') dump_print"
                    .Replace("\r\n", "\n");

            text.SimpleFormattingTest(expectedText, 100, 0);
        }

        [Test]
        [UnitTest]
        public void Reformat1_120()
        {
            const string text =
                @"systemdata:{1 type instance(); Memory:((0 type *('100' to_number_of_base 64)) mutable) instance(); !mutable FreePointer: Memory array_reference mutable; repeat: @ ^ while() then
    (
        ^ body(),
        repeat(^)
    );}; 1 = 1 then 2 else 4; 3; (Text('H') << 'allo') dump_print ";

            var expectedText =
                @"systemdata:
{
    1 type instance();
    Memory: ((0 type *('100' to_number_of_base 64)) mutable) instance();
    !mutable FreePointer: Memory array_reference mutable;

    repeat:
        @ ^ while()
        then
        (
            ^ body(),
            repeat(^)
        );
};

1 = 1 then 2 else 4;
3;
(Text('H') << 'allo') dump_print";

            text.SimpleFormattingTest(expectedText, 120, 1);
        }

        [Test]
        [UnitTest]
        public void Reformat1_120TopLineBreak()
        {
            const string text =
                @"(aaaaa 
    (
    ))";

            var expectedText =
                @"(
    aaaaa
    (
    )
)".Replace("\r\n", "\n");


            text.SimpleFormattingTest(expectedText, 120, 1);
        }
        
        [UnitTest]
        public void Reformat1_120EmptyBrackets()
        {
            const string text =
                @"(
)";



            text.SimpleFormattingTest(maxLineLength: 120, emptyLineLimit: 1);
        }

        [Test]
        [UnitTest]
        public void Reformat2()
        {
            const string text =
                @"systemdata:
{
    Memory:((0 type *(125)) mutable) instance();
    !mutable FreePointer: Memory array_reference mutable;
};

";

            const string expectedText =
                @"systemdata:
{
    Memory: ((0 type *(125)) mutable) instance();
    !mutable FreePointer: Memory array_reference mutable;
};";
            
            text.SimpleFormattingTest(expectedText, 60, 0);
        }


        [Test]
        [UnitTest]
        public void TwoLevelParenthesis()
        {
            const string text = @"aaaaa;llll:bbbbb;(cccccsssss)";
            const string expectedText = @"aaaaa;
llll: bbbbb;

(
    cccccsssss
)";

            text.SimpleFormattingTest(expectedText, 12, 1);
        }

        [Test]
        [UnitTest]
        public void UseLineBreakBeforeParenthesis()
        {
            const string Text =
                @"a:{ 1234512345, 12345}";

            var expectedText =
                @"a:
{
    1234512345,
    12345
}"
                    .Replace("\r\n", "\n");


            var compiler = CompilerBrowser.FromText(Text);
            var newSource = compiler.Reformat
                (
                    new ReniUI.Formatting.Configuration {MaxLineLength = 10, EmptyLineLimit = 0}.Create()
                )
                .Replace("\r\n", "\n");

            var lineCount = newSource.Count(item => item == '\n');

            Tracer.Assert(newSource == expectedText, "\n\"" + newSource + "\"");
        }

        [Test]
        [UnitTest]
        public void HalfList()
        {
            const string text = @"System:
(
    ssssss";
            const string expectedText = text;

            text.SimpleFormattingTest(expectedText, 12, 1);
        }

    }
}