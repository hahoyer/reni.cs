using hw.DebugFormatter;
using hw.UnitTest;

namespace ReniUI.Test
{
    [UnitTest]
    public sealed class TokenLocating : DependenceProvider
    {
        [UnitTest]
        public void FromSourcePart()
        {
            const string text = @"(1,3,4,6)";
            var compiler = CompilerBrowser.FromText(text);
            var span = (compiler.Source + 2).Span(3);
            var locateSpan = compiler.Locate(span);
            var x = locateSpan.SourcePart;

            (x.Id == "1,3,4,6").Assert(x.Dump);
        }


        [UnitTest]
        public void CommentFromSourcePart()
        {
            const string text = @"( # Comment
1,3,4,6)";
            var compiler = CompilerBrowser.FromText(text);
            var span = compiler.Source + 2;
            var x = compiler.Locate(span);
            var sourcePart = x.SourcePart;
            (sourcePart.Id.Replace("\r", "") == "# Comment\n").Assert(x.Dump);
        }
    }
}