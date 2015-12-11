using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.UnitTest;

namespace Reni.FeatureTest.UserInterface
{
    [UnitTest]
    public sealed class BraceMatching : DependantAttribute
    {
        [UnitTest]
        public void MatchingBraces()
        {
            const string Text = @"(1,3,4,6)";

            var compiler = Compiler.BrowserFromText(Text);

            var open = compiler.LocatePosition(0);
            var close = compiler.LocatePosition(Text.IndexOf(")"));

            var matchOpen = open.FindAllBelongings(compiler);
            var matchClose = close.FindAllBelongings(compiler);

            var pairs = matchOpen.Merge(matchClose, item => item).ToArray();

            Tracer.Assert(pairs.Length == 2);
        }

        [UnitTest]
        public void MoreMatchingBraces()
        {
            const string Text = @"(1,3),4,6)";

            var compiler = Compiler.BrowserFromText(Text);

            var open = compiler.LocatePosition(0);
            var close = compiler.LocatePosition(Text.IndexOf(")"));

            var matchOpen = open.FindAllBelongings(compiler);
            var matchClose = close.FindAllBelongings(compiler);

            var pairs = matchOpen.Merge(matchClose, item => item).ToArray();

            Tracer.Assert(pairs.Length == 2);
        }

        [UnitTest]
        public void NotMatchingBraces()
        {
            const string Text = @"(1,3),4,6)";

            var compiler = Compiler.BrowserFromText(Text);

            var close = compiler.LocatePosition(Text.IndexOf(")", Text.IndexOf(")")+1));

            var matchClose = close.FindAllBelongings(compiler);

            Tracer.Assert(matchClose.Count()==1);
        }
    }
}