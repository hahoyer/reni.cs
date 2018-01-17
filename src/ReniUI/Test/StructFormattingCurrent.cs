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
        public void UseSpaceWhenLinebreakIsRemoved()
        {
            const string text =
                @"(12345,12345,12345,12345,12345)";

            const string expectedText =
                @"(
    12345,
    12345,
    12345,
    12345,
    12345
)";

            var compiler = CompilerBrowser.FromText(text);
            var newSource = compiler.Reformat
                (
                    new ReniUI.Formatting.Configuration
                    {
                        EmptyLineLimit = 0,
                        MaxLineLength = 20
                    }.Create()
                )
                .Replace("\r\n", "\n");

            Tracer.Assert(newSource == expectedText, "\n\"" + newSource + "\"");
        }
    }
}