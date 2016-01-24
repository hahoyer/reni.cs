using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using hw.DebugFormatter;
using hw.UnitTest;

namespace Reni.FeatureTest.UserInterface
{
    [UnitTest]
    public sealed class ThenElseMatching : DependantAttribute
    {
        [UnitTest]
        public void Matching()
        {
            const string Text = @"1 then 2 else 3";
            var compiler = Compiler.BrowserFromText(text: Text);
            var thenToken = compiler.LocatePosition(Text.IndexOf("then"));
            var elseToken = compiler.LocatePosition(Text.IndexOf("else"));
            Tracer.Assert(elseToken.SourceSyntax.Left == thenToken.SourceSyntax);
        }

        [UnitTest]
        public void NestedMatching()
        {
            const string Text = @"1 then 2 then 333 else 3";
            var compiler = Compiler.BrowserFromText(text: Text);
            var thenToken = compiler.LocatePosition(Text.IndexOf("then"));
            var elseToken = compiler.LocatePosition(Text.IndexOf("else"));
            Tracer.Assert(elseToken.SourceSyntax.Left == thenToken.SourceSyntax);
        }
    }
}