using System;
using System.Linq;
using hw.DebugFormatter;
using Reni;
using ReniUI.Formatting;

namespace ReniUI.Test.Formatting
{
    static class Extension
    {
        public static void SimpleFormattingTest
        (
            this string text
            , string expected = null
            , int? maxLineLength = null
            , int? emptyLineLimit = null
            , CompilerParameters parameters = null
        )
        {
            var canonicalText = text.Replace("\r\n", "\n");
            expected ??= canonicalText;
            expected = expected.Replace("\r\n", "\n");

            var compiler = CompilerBrowser.FromText(canonicalText, parameters);
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
", 1
            );
        }
    }
}