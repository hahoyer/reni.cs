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
    public sealed class UserInterAction3 : DependantAttribute
    {
        const string Text = @"   !mutable FreePointer: Memory array_reference mutable;";
        const string Type = @"wwwkkkkkkkkwiiiiiiiiiiikwiiiiiiwiiiiiiiiiiiiiiiwiiiiiiik";

        [UnitTest]
        public void GetTokenForPosition()
        {
            var compiler = new Compiler(text: Text);

            var typeCharacters = new string
                (
                Text
                    .Length
                    .Select(item => compiler.Containing(item).TypeCharacter)
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