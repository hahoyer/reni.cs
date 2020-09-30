using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.UnitTest;
using Reni;

namespace ReniUI.Test
{
    [UnitTest]
    [UserInterAction]
    public sealed class UserInterAction2 : DependenceProvider
    {
        const string Text = @"#(aa comment aa)# !mutable name: 3";
        const string Type = @"cccccccccccccccccwkkkkkkkkwiiiikwn";

        [UnitTest]
        public void GetTokenForPosition()
        {
            var compiler = CompilerBrowser.FromText(text: Text);

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