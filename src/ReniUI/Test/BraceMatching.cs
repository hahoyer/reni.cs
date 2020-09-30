using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.UnitTest;

namespace ReniUI.Test
{
    [UnitTest]
    public sealed class BraceMatching : DependenceProvider
    {
        [UnitTest]
        public void MatchingBraces()
        {
            const string Text = @"(1,3,4,6)";

            var compiler = CompilerBrowser.FromText(Text);

            var open = compiler.LocatePosition(0);
            var close = compiler.LocatePosition(Text.IndexOf(")"));

            var matchOpen = compiler.FindAllBelongings(open);
            var matchClose = compiler.FindAllBelongings(close);

            var pairs = matchOpen.Merge(matchClose, item => item).ToArray();

            Tracer.Assert(pairs.Length == 2);
        }

        [UnitTest]
        public void MoreMatchingBraces()
        {
            const string Text = @"(1,3),4,6)";

            var compiler = CompilerBrowser.FromText(Text);

            var open = compiler.LocatePosition(0);
            var close = compiler.LocatePosition(Text.IndexOf(")"));

            var matchOpen = compiler.FindAllBelongings(open);
            var matchClose = compiler.FindAllBelongings(close);

            var pairs = matchOpen.Merge(matchClose, item => item).ToArray();

            Tracer.Assert(pairs.Length == 2);
        }

        [UnitTest]
        public void NotMatchingBraces()
        {
            const string Text = @"(1,3),4,6)";

            var compiler = CompilerBrowser.FromText(Text);

            var close = compiler.LocatePosition(Text.IndexOf(")", Text.IndexOf(")") + 1));

            var matchClose = compiler.FindAllBelongings(close);

            Tracer.Assert(matchClose.Count() == 1);
        }
    }
}