using System.Linq;
using hw.DebugFormatter;
using hw.UnitTest;
using NUnit.Framework;
using ReniUI.Formatting;

namespace ReniUI.Test
{
    [UnitTest]
    [StructFormatting]
    public sealed class StructFormattingCurrent : DependantAttribute
    {
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
                    new ReniUI.Formatting.Configuration
                    {
                        MaxLineLength = 10,
                        EmptyLineLimit = 0
                    }.Create()
                )
                .Replace("\r\n", "\n");

            var lineCount = newSource.Count(item => item == '\n');

            Tracer.Assert(newSource == expectedText, "\n\"" + newSource + "\"");
        }
    }
}