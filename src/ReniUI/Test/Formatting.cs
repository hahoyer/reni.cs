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
    public sealed class Formatting : DependantAttribute
    {
        [Test]
        [UnitTest]
        public void LabelsOnToLevel()
        {
            const string text = @"aaaaa;ccccc";
            const string expectedText = @"aaaaa; ccccc";

            var compiler = CompilerBrowser.FromText(text);
            var newSource = compiler.Syntax.FlatFormat(emptyLineLimit: 0);
            Tracer.Assert(newSource == expectedText, "\n\"" + newSource + "\"");
        }

        [Test]
        [UnitTest]
        public void FlatFormatTest()
        {
            const string text = @"aaaaa;llll:bbbbb;ccccc";
            const string expectedText = @"aaaaa;
llll: bbbbb;
ccccc";

            text.SimpleTest(expectedText, maxLineLength: 10, emptyLineLimit: 1);
        }

        [Test]
        [UnitTest]
        public void SingleElementList()
        {
            const string text = @"(aaaaaddddd)";
            const string expectedText = @"(
    aaaaaddddd
)";

            text.SimpleTest(expectedText, maxLineLength: 10, emptyLineLimit: 1);
        }

        [Test]
        [UnitTest]
        public void SingleElementListFlat()
        {
            const string text = @"(aaaaaddddd)";
            const string expectedText = @"(aaaaaddddd)";

            text.SimpleTest(expectedText, maxLineLength: 14, emptyLineLimit: 1);
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
            text.SimpleTest(expectedText, maxLineLength: 10, emptyLineLimit: 1);
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

            var compiler = CompilerBrowser.FromText(text);
            var newSource = compiler.Reformat
            (
                new ReniUI.Formatting.Configuration {MaxLineLength = 10, EmptyLineLimit = 1}.Create()
            );

            Tracer.Assert(newSource == expectedText, "\n\"" + newSource + "\"");
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
        public void TwoLevelParenthesis()
        {
            const string text = @"aaaaa;llll:bbbbb;(cccccsssss)";
            const string expectedText = @"aaaaa;
llll: bbbbb;
(
    cccccsssss
)";

            text.SimpleTest(expectedText, maxLineLength: 10, emptyLineLimit: 1);
        }

        [Test]
        [UnitTest]
        public void Reformat()
        {
            const string Text =
                @"systemdata:{1 type instance(); Memory:((0 type *('100' to_number_of_base 64)) mutable) instance(); !mutable FreePointer: Memory array_reference mutable; repeat: /\ ^ while() then
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
    repeat: /\ ^ while() then(^ body(), repeat(^));
};
1 = 1 then 2 else 4;
3;
(Text('H') << 'allo') dump_print"
                    .Replace(oldValue: "\r\n", newValue: "\n");


            var compiler = CompilerBrowser.FromText(Text);
            var newSource = compiler.Reformat
                (
                    new ReniUI.Formatting.Configuration {MaxLineLength = 100, EmptyLineLimit = 0}.Create()
                )
                .Replace(oldValue: "\r\n", newValue: "\n");

            var lineCount = newSource.Count(item => item == '\n');

            Tracer.Assert(newSource == expectedText, "\n\"" + newSource + "\"");
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
)".Replace(oldValue: "\r\n", newValue: "\n");

            text.SimpleTest(expectedText, maxLineLength: 120, emptyLineLimit: 1);
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
)".Replace(oldValue: "\r\n", newValue: "\n");

            text.SimpleTest(expectedText, maxLineLength: 10, emptyLineLimit: 1);
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
)".Replace(oldValue: "\r\n", newValue: "\n");

            text.SimpleTest(expectedText, maxLineLength: 10, emptyLineLimit: 1);
        }
        
        [Test]
        [UnitTest]
        public void Reformat1_120()
        {
            const string text =
                @"systemdata:{1 type instance(); Memory:((0 type *('100' to_number_of_base 64)) mutable) instance(); !mutable FreePointer: Memory array_reference mutable; repeat: /\ ^ while() then
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

    repeat: /\ ^ while() then
    (
        ^ body(),
        repeat(^)
    );
};

1 = 1 then 2 else 4;
3;
(Text('H') << 'allo') dump_print";

            text.SimpleTest(expectedText, maxLineLength: 120, emptyLineLimit: 1);
        }

        [Test]
        [UnitTest]
        public void Reformat2()
        {
            const string Text =
                @"systemdata:
{
    Memory:((0 type *(125)) mutable) instance();
    !mutable FreePointer: Memory array_reference mutable;
};

";

            const string ExpectedText =
                @"systemdata:
{
    Memory: ((0 type *(125)) mutable) instance();
    !mutable FreePointer: Memory array_reference mutable;
};";
            var compiler = CompilerBrowser.FromText(Text);
            var newSource = compiler.Reformat(new ReniUI.Formatting.Configuration().Create());

            var lineCount = newSource.Count(item => item == '\n');

            Tracer.Assert
            (
                newSource.Replace
                    (oldValue: "\r\n", newValue: "\n") ==
                ExpectedText.Replace(oldValue: "\r\n", newValue: "\n"),
                "\n\"" + newSource + "\"");
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
            Tracer.Assert(lineCount == 18, "\n\"" + newSource + "\"");
        }

        [Test]
        [UnitTest]
        public void ReformatWithDeclarationTag()
        {
            const string Text =
                @"!mutable FreePointer: Memory array_reference mutable; 
repeat: /\ ^ while() then
    (
        ^ body(),
        repeat(^)
    ); 1 = 1 then 2 else 4; 3; (Text('H') << 'allo') dump_print ";

            var expectedText =
                @"systemdata:
{
    1 type instance();
    Memory: ((0 type *('100' to_number_of_base 64)) mutable) instance();
    !mutable FreePointer: Memory array_reference mutable;
    repeat: /\ ^ while() then(^ body(), repeat(^));
};
1 = 1 then 2 else 4;
3;
(Text('H') << 'allo') dump_print"
                    .Replace(oldValue: "\r\n", newValue: "\n");


            var compiler = CompilerBrowser.FromText(Text);
            var newSource = compiler.Reformat
                (
                    new ReniUI.Formatting.Configuration {MaxLineLength = 100, EmptyLineLimit = 0}.Create()
                )
                .Replace(oldValue: "\r\n", newValue: "\n");

            var lineCount = newSource.Count(item => item == '\n');

            Tracer.Assert(newSource == expectedText, "\n\"" + newSource + "\"");
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
                    .Replace(oldValue: "\r\n", newValue: "\n");


            var compiler = CompilerBrowser.FromText(Text);
            var newSource = compiler.Reformat
                (
                    new ReniUI.Formatting.Configuration {MaxLineLength = 10, EmptyLineLimit = 0}.Create()
                )
                .Replace(oldValue: "\r\n", newValue: "\n");

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
                .Replace(oldValue: "\r\n", newValue: "\n");

            var compiler = CompilerBrowser.FromText(text);
            var newSource = compiler.Reformat
                (
                    new ReniUI.Formatting.Configuration {EmptyLineLimit = 0, MaxLineLength = 20}.Create()
                )
                .Replace(oldValue: "\r\n", newValue: "\n");

            Tracer.Assert(newSource == expectedText, "\n\"" + newSource + "\"");
        }
    }
}