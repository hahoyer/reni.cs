using System;
using hw.DebugFormatter;
using hw.Scanner;
using hw.UnitTest;
using NUnit.Framework;
using Reni.Parser;

namespace Reni.ParserTest
{
    [TestFixture]
    [UnitTest]
    public sealed class Comments
    {
        [Test]
        [UnitTest]
        public void MultiLineMinimal() => PerformTest("#()# outside", 4);
        [Test]
        [UnitTest]
        public void MultiLineOneSpace() => PerformTest("#( )# outside", 5);

        static void PerformTest(string text, int expectedLength)
        {
            var source = new Source(text) + 0;
            var length = Lexer.Instance.MultiLineCommentItem.Match(source);
            (length == expectedLength).Assert();
        }
    }
}