using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using Reni;
using ReniUI.Formatting;

namespace ReniUI.Test
{
    static class Extension
    {
        public static void SimpleTest
        (
            this string text,
            string expected = null,
            int? maxLineLength = null,
            int? emptyLineLimit = null)
        {
            expected ??= text;
            expected = expected.Replace(oldValue: "\r\n", newValue: "\n");

            var compiler = CompilerBrowser.FromText(text);
            var newSource = compiler.Reformat
                (
                    new ReniUI.Formatting.Configuration {MaxLineLength = maxLineLength, EmptyLineLimit = emptyLineLimit}.Create()
                )
                .Replace(oldValue: "\r\n", newValue: "\n");

            var lineCount = newSource.Count(item => item == '\n');

            (newSource == expected).Assert(()=> $@"
newSource:
{newSource.Quote()}
expected:
{expected.Quote()}
",1);
        }
    }
}