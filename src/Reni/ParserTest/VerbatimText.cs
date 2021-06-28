using hw.DebugFormatter;
using hw.Scanner;
using hw.UnitTest;
using Reni.Parser;

namespace Reni.ParserTest
{
    [UnitTest]
    public sealed class VerbatimText
    {
        [UnitTest]
        public void Simple()
        {
            var text = "@( ertzu )@";
            var source = new Source(text) + 0;
            var length = Lexer.Instance.MatchText(source);
            Tracer.Assert(length != null && length.Value == text.Length);
        }

        [UnitTest]
        public void Extraction()
        {
            var textContainerText = "   @( ertzu )@     ";
            var source = (new Source(textContainerText) + 3).Span(11);
            var text = Lexer.Instance.ExtractText(source);
            Tracer.Assert(text == "ertzu");
        }
    }
}