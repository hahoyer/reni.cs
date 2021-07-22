using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.UnitTest;
using NUnit.Framework;

namespace ReniUI.Test.UserInteraction
{
    [TestFixture]
    [UnitTest]
    [UserInterAction]
    public sealed class UserInterAction2 : DependenceProvider
    {
        const string Text = @"#(aa comment aa)# !mutable name: 3";
        const string Type = @"cccccccccccccccccwkkkkkkkkwiiiikwn";

        [Test]
        [UnitTest]
        public void GetTokenForPosition()
        {
            var compiler = CompilerBrowser.FromText(Text);

            var typeCharacters = new string
            (
                Text
                    .Length
                    .Select(item => compiler.LocatePosition((int) item).TypeCharacter)
                    .ToArray()
            );
            (Type == typeCharacters).Assert
            (() =>
                "\nXpctd: " +
                Type +
                "\nFound: " +
                typeCharacters +
                "\nText : " +
                Text
            );
        }
    }
}