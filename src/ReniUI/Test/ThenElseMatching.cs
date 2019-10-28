using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.UnitTest;
using Reni;

namespace ReniUI.Test
{
    [UnitTest]
    public sealed class ThenElseMatching : DependantAttribute
    {
        [UnitTest]
        public void Matching()
        {
            const string Text = @"1 then 2 else 3";
            var compiler = CompilerBrowser.FromText(text: Text);
            var thenToken = compiler.LocatePosition(Text.IndexOf("then"));
            var elseToken = compiler.LocatePosition(Text.IndexOf("else"));
            Tracer.Assert(elseToken.Syntax.Left == thenToken.Syntax);
        }

        [UnitTest]
        public void NestedMatching()
        {
            const string Text = @"1 then 2 then 333 else 3";
            var compiler = CompilerBrowser.FromText(text: Text);
            var thenToken = compiler.LocatePosition(Text.IndexOf("then"));
            var elseToken = compiler.LocatePosition(Text.IndexOf("else"));
            Tracer.Assert(elseToken.Syntax.Left == thenToken.Syntax);
        }
    }
}