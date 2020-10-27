using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.UnitTest;
// ReSharper disable StringIndexOfIsCultureSpecific.1
// ReSharper disable StringIndexOfIsCultureSpecific.2

namespace ReniUI.Test
{
    [UnitTest]
    public sealed class PairedSyntaxTree : DependenceProvider
    {
        [UnitTest]
        public void SimpleList()
        {
            const string text = @"(1)";

            var compiler = CompilerBrowser.FromText(text);

            var open = compiler.LocatePosition(0);
            var close = compiler.LocatePosition(text.IndexOf(")"));

            var matchOpen = open.ParserLevelBelongings;
            var matchClose = close.ParserLevelBelongings;

            var pairs = matchOpen.Merge(matchClose, item => item).ToArray();

            (pairs.Length == 2).Assert(pairs.Dump);
        }

        [UnitTest]
        public void TopLevelList()
        {
            const string text = @"1,3";

            var compiler = CompilerBrowser.FromText(text);

            var open = compiler.LocatePosition(0);
            var close = compiler.LocatePosition(text.IndexOf(")"));

            var matchOpen = open.ParserLevelBelongings;
            var matchClose = close.ParserLevelBelongings;

            var pairs = matchOpen.Merge(matchClose, item => item).ToArray();

            (pairs.Length == 2).Assert();
        }

        [UnitTest]
        public void List()
        {
            const string text = @"(1,3)";

            var compiler = CompilerBrowser.FromText(text);

            var open = compiler.LocatePosition(0);
            var close = compiler.LocatePosition(text.IndexOf(")"));

            var matchOpen = open.ParserLevelBelongings;
            var matchClose = close.ParserLevelBelongings;

            var pairs = matchOpen.Merge(matchClose, item => item).ToArray();

            (pairs.Length == 2).Assert();
        }
    }
}