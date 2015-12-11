using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.UnitTest;

namespace Reni.FeatureTest.UserInterface
{
    [UnitTest]
    [UserInterAction]
    public sealed class UserInterAction3 : DependantAttribute
    {
        const string Text = @"   !mutable FreePointer: Memory array_reference mutable;";
        const string Type = @"wwwkkkkkkkkwiiiiiiiiiiikwiiiiiiwiiiiiiiiiiiiiiiwiiiiiiik";

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