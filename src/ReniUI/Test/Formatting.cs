using System.Diagnostics;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
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
                    .Replace("\r\n", "\n");


            var compiler = CompilerBrowser.FromText(Text);
            var newSource = compiler.Reformat
                (
                    new ReniUI.Formatting.Configuration
                    {
                        MaxLineLength = 100,
                        EmptyLineLimit = 0
                    }.Create()
                )
                .Replace("\r\n", "\n");

            var lineCount = newSource.Count(item => item == '\n');

            Tracer.Assert(newSource == expectedText, "\n\"" + newSource + "\"");
        }

        [Test]
        [UnitTest]
        public void Reformat1_120()
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

    repeat: /\ ^ while() then
    (
        ^ body(),
        repeat(^)
    );
};

1 = 1 then 2 else 4;
3;
(Text('H') << 'allo') dump_print"
                    .Replace("\r\n", "\n");


            var compiler = CompilerBrowser.FromText(Text);
            var newSource = compiler.Reformat
                (
                    new ReniUI.Formatting.Configuration
                    {
                        MaxLineLength = 120,
                        EmptyLineLimit = 1
                    }.Create()
                )
                .Replace("\r\n", "\n");

            var lineCount = newSource.Count(item => item == '\n');

            Tracer.Assert(newSource == expectedText, "\n\"" + newSource + "\"");
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
                newSource.Replace("\r\n", "\n") == ExpectedText.Replace("\r\n", "\n"),
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
                new ReniUI.Formatting.Configuration
                    {
                        EmptyLineLimit = 1
                    }.Create
                    ());

            var lineCount = newSource.Count(item => item == '\n');
            Tracer.Assert(lineCount == 18, "\n\"" + newSource + "\"");
        }

        [Test]
        [UnitTest]
        public void SimpleLineCommentFromSourcePart()
        {
            const string Text = @"(1,3,4,6)";
            var compiler = CompilerBrowser.FromText(Text);
            var span = compiler.Source.All;
            var trimmed = compiler.Reformat(sourcePart: span);

            Tracer.Assert(trimmed == "(1, 3, 4, 6)", trimmed);
        }
    }
}