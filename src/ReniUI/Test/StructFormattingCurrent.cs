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
        public void OmitSpaceWhenLinebreakRemains()
        {
            const string text =
                @"a
b";

            var expectedText = @"aa
b"              
                .Replace("\r\n", "\n");
            ;

            var compiler = CompilerBrowser.FromText(text);
            var newSource = compiler.Reformat
                (
                    new ReniUI.Formatting.Configuration
                    {
                        EmptyLineLimit = null
                    }.Create()
                )
                .Replace("\r\n", "\n");

            Tracer.Assert(newSource == expectedText, "\n\"" + newSource + "\"");
        }
    }
}