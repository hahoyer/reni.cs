using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using hw.UnitTest;

namespace Reni.FeatureTest.UserInterface
{
    [UnitTest]
    [UserInterAction]
    public sealed class UserInterAction2 : DependantAttribute
    {
        const string Text = @"#(aa comment aa)# !mutable name: 3";
        const string Type = @"cccccccccccccccccwkkkkkkkkwiiiikwn";

        [UnitTest]
        public void GetTokenForPosition()
        {
            var compiler = Compiler.BrowserFromText(text: Text);

            var typeCharacters = new string
                (
                Text
                    .Length
                    .Select(item => compiler.LocatePosition(item).TypeCharacter)
                    .ToArray());
            Tracer.Assert
                (
                    Type == typeCharacters,
                    () =>
                        "\nXpctd: " + Type +
                            "\nFound: " + typeCharacters +
                            "\nText : " + Text);
        }

    }
}