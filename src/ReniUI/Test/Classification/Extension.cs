using ReniUI;

namespace reniUI.Test.Classification;

static class Extension
{
    public static void GetTokenForPosition(this string text, string expected)
    {
        var compiler = CompilerBrowser.FromText(text);

        var typeCharacters = new string
        (
            text
                .Length
                .Select(item => compiler.GetToken(item).TypeCharacter)
                .ToArray()
        );
        (expected == typeCharacters).Assert
        (() =>
            "\nXpctd: " + expected + "\nFound: " + typeCharacters + "\nText : " + text
        );
    }
}