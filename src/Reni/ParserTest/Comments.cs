using hw.Scanner;
using hw.UnitTest;
using Reni.Parser;

namespace Reni.ParserTest;

[UnitTest]
public sealed class Comments
{
    [UnitTest]
    public static void MultiLineMinimal() => PerformTest("#()# outside", 4);

    [UnitTest]
    public static void MultiLineOneSpace() => PerformTest("#( )# outside", 5);

    static void PerformTest(string text, int expectedLength)
    {
        var source = new Source(text) + 0;
        var length = Lexer.Instance.InlineCommentItem.Match(source);
        (length == expectedLength).Assert();
    }
}