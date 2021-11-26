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
        public void MultiLineMinimal()
        {
            var text = " #()# outside";
            var source = new Source(text) + 0;

            var xx = Lexer.Instance.MultiLineCommentItem.Match(source);
        }
    }
}