using hw.DebugFormatter;
using hw.UnitTest;
using NUnit.Framework;
using ReniUI.Formatting;

namespace ReniUI.Test
{
    [UnitTest]
    [TestFixture]
    public sealed class StructFormatting : DependantAttribute
    {
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
            

            var compiler = CompilerBrowser.FromText(text);
            var newSource = compiler.Reformat
                (
                    new ReniUI.Formatting.Configuration
                        {EmptyLineLimit = null}.Create()
                )
                .Replace("\r\n", "\n");

            Tracer.Assert(newSource == expectedText, "\n\"" + newSource + "\"");
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

            Tracer.Assert(newSource == expectedText, "\n\"" + newSource + "\"");
        }
    }
}