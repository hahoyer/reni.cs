using hw.DebugFormatter;
using Reni;
using ReniUI.Formatting;

namespace ReniUI.Test.Formatting
{
    static class Extension
    {
        static string Canonize(this string target, char spaceReplacement = '_', bool replaceCarriageReturn = true)
        {
            if(replaceCarriageReturn)
                target = target.Replace("\r\n", "\n");
            if(spaceReplacement != '\0')
                target = target
                    .Replace(" ", "")
                    .Replace('_', ' ');
            return target;
        }

        public static void SimpleFormattingTest
        (
            this string text
            , string expected = null
            , int? maxLineLength = null
            , int? emptyLineLimit = null
            , CompilerParameters parameters = null
            , char spaceReplacement = '\0'
        )
        {
            expected ??= text;
            expected = expected.Canonize(spaceReplacement);

            var compiler = CompilerBrowser.FromText(text.Canonize(spaceReplacement, false), parameters);
            var newText = compiler.Reformat
            (
                new ReniUI.Formatting.Configuration { MaxLineLength = maxLineLength, EmptyLineLimit = emptyLineLimit }
                    .Create()
            );

            (newText.Canonize(spaceReplacement) == expected.Canonize(spaceReplacement))
                .Assert(() => $@"
new:
----------------------
{newText}
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