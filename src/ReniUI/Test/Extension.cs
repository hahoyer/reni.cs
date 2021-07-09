using System.Linq;
using System.Reflection.Emit;
using hw.DebugFormatter;
using ReniUI.Formatting;

namespace ReniUI.Test
{
    static class Extension
    {
        public static void SimpleFormattingTest
        (
            this string text,
            string expected = null,
            int? maxLineLength = null,
            int? emptyLineLimit = null
        )
        {
            var canonicalText = text.Replace("\r\n", "\n");
            expected ??= canonicalText;
            expected = expected.Replace("\r\n", "\n");

            var compiler = CompilerBrowser.FromText(canonicalText);
            var newSource = compiler.Reformat
                (
                    new ReniUI.Formatting.Configuration {MaxLineLength = maxLineLength, EmptyLineLimit = emptyLineLimit}
                        .Create()
                )
                .Replace("\r\n", "\n");

            var lineCount = newSource.Count(item => item == '\n');

            (newSource == expected).Assert(() => $@"
newSource:
----------------------
{newSource}
----------------------
expected:
----------------------
{expected}
----------------------
", 1);
        }
    }
}