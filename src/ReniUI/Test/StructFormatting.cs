using hw.DebugFormatter;
using hw.UnitTest;
using ReniUI.Formatting;

namespace ReniUI.Test
{
    [UnitTest]
    public sealed class StructFormatting : DependantAttribute
    {
        [UnitTest]
        public void OmitSpaceWhenLinebreakRemains()
        {
            const string text =
                @"a
b";

            var expectedText = @"a
    b"
                .Replace("\r\n", "\n");
            ;

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
        public void UseSpaceWhenLinebreakIsRemoved()
        {
            const string text =
                @"a
b";

            var expectedText = @"a b".Replace("\r\n", "\n");
            ;

            var compiler = CompilerBrowser.FromText(text);
            var newSource = compiler.Reformat
                (
                    new ReniUI.Formatting.Configuration
                        {EmptyLineLimit = 0}.Create()
                )
                .Replace("\r\n", "\n");

            Tracer.Assert(newSource == expectedText, "\n\"" + newSource + "\"");
        }
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

            Tracer.Assert(newSource == expectedText, "\n\"" + newSource + "\"");
        }
    }
}