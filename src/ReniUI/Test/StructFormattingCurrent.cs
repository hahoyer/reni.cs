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
        public void LongChainInList()
        {
            const string text =
                @"method member function(parameter1, parameter2, parameter3), thing2()";

            var expectedText = @"method
    member
    function
    (
        parameter1,
        parameter2,
        parameter3
    ),
thing2()"
                .Replace("\r\n", "\n");

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

            var both = newSource + "\n==\n" + expectedText;
            Tracer.Assert(newSource == expectedText, "\n\"" + newSource + "\"");
        }
    }
}