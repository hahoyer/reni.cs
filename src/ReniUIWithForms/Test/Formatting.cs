using System.Linq;
using hw.DebugFormatter;
using hw.UnitTest;
using NUnit.Framework;
using ReniUI.Formatting;

namespace ReniUI.Test
{
    [UnitTest]
    [TestFixture]
    [FormattingSimple]
    public sealed class Formatting : DependenceProvider
    {
        [Test]
        [UnitTest]
        public void HalfList()
        {
            const string text = @"System:
(
    ssssss";
            const string expectedText = text;

            text.SimpleTest(expectedText, 12, 1);
        }

        [Test]
        [UnitTest]
        public void LabeledList()
        {
            const string text = @"llll:(aaaaa;llll:bbbbb;ccccc)";
            const string expectedText = @"llll:
(
    aaaaa;
    llll: bbbbb;
    ccccc
)";

            text.SimpleTest(expectedText, 12, 1);
        }

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
        public void ListEndsWithListToken()
        {
            const string text = @"(aaaaa;bbbbb;ccccc;)";
            const string expectedText = @"(
    aaaaa;
    bbbbb;
    ccccc;
)";

            var compiler = CompilerBrowser.FromText(text);
            var newSource = compiler.Reformat
            (
                new ReniUI.Formatting.Configuration {MaxLineLength = 20, EmptyLineLimit = 1}.Create()
            );

            Tracer.Assert(newSource == expectedText, "\n\"" + newSource + "\"");
        }

        [Test]
        [UnitTest]
        public void ListSeparatorAtEnd()
        {
            const string text = @"aaaaa;";
            const string expectedText = @"aaaaa;";

            var compiler = CompilerBrowser.FromText(text);
            var newSource = compiler.FlatFormat(false);
            Tracer.Assert(newSource == expectedText, "\n\"" + newSource + "\"");
        }

        [Test]
        [UnitTest]
        public void ListLineBreakTest2()
        {
            const string text = @"aaaaa;bbbbb";
            const string expectedText = @"aaaaa;
bbbbb";

            text.SimpleTest(expectedText, 10, 1);
        }

        [Test]
        [UnitTest]
        public void ListLineBreakTest3()
        {
            const string text = @"aaaaa;bbbbb;ccccc";
            const string expectedText = @"aaaaa;
bbbbb;
ccccc";

            text.SimpleTest(expectedText, 10, 1);
        }

        [Test]
        [UnitTest]
        public void ListTest1()
        {
            const string text = @"aaaaa";
            const string expectedText = @"aaaaa";

            text.SimpleTest(expectedText, 10, 1);
        }

        [Test]
        [UnitTest]
        public void ListTest2()
        {
            const string text = @"aaaaa;bbbbb";
            const string expectedText = @"aaaaa; bbbbb";

            text.SimpleTest(expectedText, 20, 1);
        }

        [Test]
        [UnitTest]
        public void ListWithDeclarationLineBreakTest1()
        {
            const string text = @"lllll:bbbbb";
            const string expectedText = @"lllll:
    bbbbb";

            text.SimpleTest(expectedText, 10, 1);
        }

        [Test]
        [UnitTest]
        public void ListWithDeclarationLineBreakTest2()
        {
            const string text = @"aaaaa;llll:bbbbb";
            const string expectedText = @"aaaaa;
llll: bbbbb";

            text.SimpleTest(expectedText, 15, 1);
        }

        [Test]
        [UnitTest]
        public void ListWithDeclarationLineBreakTest3()
        {
            const string text = @"aaaaa;llll:bbbbb;ccccc";
            const string expectedText = @"aaaaa;
llll: bbbbb;
ccccc";

            text.SimpleTest(expectedText, 20, 1);
        }

        [Test]
        [UnitTest]
        public void ListWithDeclarationLineBreakTestA2()
        {
            const string text = @"aaaaa;llll:bbbbb";
            const string expectedText = @"aaaaa;

llll:
    bbbbb";

            text.SimpleTest(expectedText, 10, 1);
        }

        [Test]
        [UnitTest]
        public void ListWithDeclarationLineBreakTestA3()
        {
            const string text = @"aaaaa;llll:bbbbb;ccccc";
            const string expectedText = @"aaaaa;

llll:
    bbbbb;

ccccc";

            text.SimpleTest(expectedText, 10, 1);
        }

        [Test]
        [UnitTest]
        public void ListWithDeclarationLineBreakTestA4()
        {
            const string text = @"aaaaa;aaaaa;llll:bbbbb;ccccc;ddddd";
            const string expectedText = @"aaaaa;
aaaaa;

llll:
    bbbbb;

ccccc;
ddddd";

            text.SimpleTest(expectedText, 10, 1);
        }

        [Test]
        [UnitTest]
        public void ListWithDeclarationLineBreakTestAA4()
        {
            const string text = @"aaaaa;aaaaa;llll:bbbbb;mmmmm:22222;ccccc;ddddd";
            const string expectedText = @"aaaaa;
aaaaa;

llll:
    bbbbb;

mmmmm:
    22222;

ccccc;
ddddd";

            text.SimpleTest(expectedText, 10, 1);
        }

        [Test]
        [UnitTest]
        public void ListWithDeclarationTest1()
        {
            const string text = @"lllll:bbbbb";
            const string expectedText = @"lllll: bbbbb";

            text.SimpleTest(expectedText, emptyLineLimit: 1);
        }

        [Test]
        [UnitTest]
        public void ListWithDeclarationTest2()
        {
            const string text = @"aaaaa;llll:bbbbb";
            const string expectedText = @"aaaaa; llll: bbbbb";

            text.SimpleTest(expectedText, emptyLineLimit: 1);
        }

        [Test]
        [UnitTest]
        public void ListWithDeclarationTest3()
        {
            const string text = @"aaaaa;llll:bbbbb;ccccc";
            const string expectedText = @"aaaaa; llll: bbbbb; ccccc";

            text.SimpleTest(expectedText, emptyLineLimit: 1);
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

            text.SimpleTest(expectedText, 10, 1);
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

            text.SimpleTest(expectedText, 10, 1);
        }

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

            text.SimpleTest(expectedText, 100, 0);
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

            text.SimpleTest(expectedText, 120, 1);
        }

        [Test]
        [UnitTest]
        public void Reformat1_120Temp()
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

            text.SimpleTest(expectedText, 120, 1);
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
            
            text.SimpleTest(expectedText, 60, 0);
        }

        [Test]
        [UnitTest]
        public void ReformatWithComments()
        {
            const string Text = @"
    X1_fork: object
    (

            X1_FullDump:
            (
            function
            (
                string('(') * X1_L X1_dump(arg) * ',' * X1_R X1_dump(arg) * ')';
        )
        );

            X1_dump:=
           (
           function
           (
           arg = 0 then string('...') else X1_FullDump(integer(arg - 1))
           )
           )
    );
            ";

            var compiler = CompilerBrowser.FromText(Text);
            var newSource = compiler.Reformat
            (
                new ReniUI.Formatting.Configuration {EmptyLineLimit = 1}.Create
                    ());

            var lineCount = newSource.Count(item => item == '\n');
            Tracer.Assert(lineCount == 22, "\n\"" + newSource + "\"");
        }

        [Test]
        [UnitTest]
        public void SimpleLineCommentFromSourcePart()
        {
            const string Text = @"(1,3,4,6)";
            var compiler = CompilerBrowser.FromText(Text);
            var span = compiler.Source.All;
            var trimmed = compiler.Reformat(targetPart: span);

            Tracer.Assert(trimmed == "(1, 3, 4, 6)", trimmed);
        }

        [Test]
        [UnitTest]
        public void SingleElementList()
        {
            const string text = @"(aaaaaddddd)";
            const string expectedText = @"(
    aaaaaddddd
)";

            text.SimpleTest(expectedText, 10, 1);
        }

        [Test]
        [UnitTest]
        public void SingleElementListFlat()
        {
            const string text = @"(aaaaaddddd)";
            const string expectedText = @"(aaaaaddddd)";

            text.SimpleTest(expectedText, 14, 1);
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
            text.SimpleTest(expectedText, 10, 1);
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

            text.SimpleTest(expectedText, 12, 1);
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
        public void UseSpaceWhenLineBreakIsRemoved()
        {
            const string text =
                @"(12345,12345,12345,12345,12345)";

            var expectedText = @"(
    12345,
    12345,
    12345,
    12345,
    12345
)"
                .Replace("\r\n", "\n");

            var compiler = CompilerBrowser.FromText(text);
            var newSource = compiler.Reformat
                (
                    new ReniUI.Formatting.Configuration {EmptyLineLimit = 0, MaxLineLength = 20}.Create()
                )
                .Replace("\r\n", "\n");

            Tracer.Assert(newSource == expectedText, "\n\"" + newSource + "\"");
        }
    }
}