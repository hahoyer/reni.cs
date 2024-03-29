using hw.DebugFormatter;
using hw.UnitTest;
using NUnit.Framework;

namespace ReniUI.Test
{
    [UnitTest]
    [TestFixture]
    public sealed class TokenLocating : DependenceProvider
    {
        [UnitTest]
        public void FromSourcePart()
        {
            const string text = @"(1,3,4,6)";
            var compiler = CompilerBrowser.FromText(text);
            var span = (compiler.Source + 2).Span(3);
            var x = compiler.LocateIncludingParent(span).SourcePart;

            (x.Id == "1,3,4,6").Assert(x.Dump);
        }


        [UnitTest]
        [Test]
        public void CommentFromSourcePart()
        {
            const string text = @"( # Comment
1,3,4,6)";
            var compiler = CompilerBrowser.FromText(text);
            var span = compiler.Source + 2;
            var x = compiler.LocatePosition(span);
            var sourcePart = x.SourcePart;
            (sourcePart.Id.Replace("\r", "") == "# Comment\n").Assert(x.Dump);
        }


        [UnitTest]
        [Test]
        public void CompoundSourcePart()
        {
            const string text = @"(1,3,4,6)";
            var compiler = CompilerBrowser.FromText(text);
            var span = compiler.Source + text.IndexOf(',');
            var x = compiler.LocatePosition(span).Master.Anchors.Combine();
            (x.Id.Replace("\r", "") == "1,3,4,6").Assert(x.Dump);
        }

        [UnitTest]
        [Test]
        public void NamedCompoundSourcePart()
        {
            const string text = @"(x:1,3,4,6)";
            var compiler = CompilerBrowser.FromText(text);
            var span = compiler.Source + text.IndexOf(',');
            var x = compiler.LocatePosition(span).Master.Anchors.Combine();
            (x.Id.Replace("\r", "") == "x:1,3,4,6").Assert(x.Dump);
        }
    }
}