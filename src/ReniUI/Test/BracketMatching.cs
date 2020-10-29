using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.UnitTest;

// ReSharper disable StringIndexOfIsCultureSpecific.1
// ReSharper disable StringIndexOfIsCultureSpecific.2

namespace ReniUI.Test
{
    [UnitTest]
    [PairedSyntaxTree]
    public sealed class BracketMatching : DependenceProvider
    {
        [UnitTest]
        public void MatchingBraces()
        {
            const string text = @"(1,3,4,6)";

            var compiler = CompilerBrowser.FromText(text);

            var open = compiler.LocatePosition(0);
            var close = compiler.LocatePosition(text.IndexOf(")"));

            var matchOpen = open.ParserLevelGroup;
            var matchClose = close.ParserLevelGroup;

            var pairs = matchOpen.Merge(matchClose, item => item).ToArray();

            (pairs.Length == 2).Assert(pairs.Dump);
        }

        [UnitTest]
        public void MoreMatchingBraces()
        {
            const string text = @"(1,3),4,6)";

            var compiler = CompilerBrowser.FromText(text);

            var open = compiler.LocatePosition(0);
            var close = compiler.LocatePosition(text.IndexOf(")"));

            var matchOpen = open.ParserLevelGroup.ToArray();
            var matchClose = close.ParserLevelGroup.ToArray();

            var pairs = matchOpen.Merge(matchClose, item => item).ToArray();

            (matchOpen.Length == 1).Assert();
            (matchClose.Length == 1).Assert();
        }

        [UnitTest]
        public void NotMatchingBraces()
        {
            const string text = @"(1,3),4,6)";

            var compiler = CompilerBrowser.FromText(text);

            var close = compiler.LocatePosition(text.IndexOf(")", text.IndexOf(")") + 1));

            var matchClose = close.ParserLevelGroup;

            (!matchClose.Any()).Assert();
        }
    }
}