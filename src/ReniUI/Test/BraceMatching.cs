using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.UnitTest;

// ReSharper disable StringIndexOfIsCultureSpecific.1
// ReSharper disable StringIndexOfIsCultureSpecific.2

namespace ReniUI.Test
{
    [UnitTest]
    public sealed class BraceMatching : DependenceProvider
    {
        [UnitTest]
        public void MatchingBraces()
        {
            const string text = @"(1,3,4,6)";

            var compiler = CompilerBrowser.FromText(text);

            var open = compiler.LocatePosition(0);
            var close = compiler.LocatePosition(text.IndexOf(")"));

            var matchOpen = compiler.FindAllBelongings(open);
            var matchClose = compiler.FindAllBelongings(close);

            var pairs = matchOpen.Merge(matchClose, item => item).ToArray();

            Tracer.Assert(pairs.Length == 2);
        }

        [UnitTest]
        public void MoreMatchingBraces()
        {
            const string text = @"(1,3),4,6)";

            var compiler = CompilerBrowser.FromText(text);

            var open = compiler.LocatePosition(0);
            var close = compiler.LocatePosition(text.IndexOf(")"));

            var matchOpen = compiler.FindAllBelongings(open);
            var matchClose = compiler.FindAllBelongings(close);

            var pairs = matchOpen.Merge(matchClose, item => item).ToArray();

            Tracer.Assert(pairs.Length == 2);
        }

        [UnitTest]
        public void NotMatchingBraces()
        {
            const string text = @"(1,3),4,6)";

            var compiler = CompilerBrowser.FromText(text);

            var close = compiler.LocatePosition(text.IndexOf(")", text.IndexOf(")") + 1));

            var matchClose = compiler.FindAllBelongings(close);

            Tracer.Assert(matchClose.Count() == 1);
        }
    }
}