using ReniUI;

namespace reniUI.Test.Classification;

static class Extension
{
    extension(string text)
    {
        public void GetTokenForPosition(string expected)
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
}