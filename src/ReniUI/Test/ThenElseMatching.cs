using hw.DebugFormatter;
using hw.UnitTest;

namespace ReniUI.Test
{
    [UnitTest]
    public sealed class ThenElseMatching : DependenceProvider
    {
        [UnitTest]
        public void Matching()
        {
            const string Text = @"1 then 2 else 3";
            var compiler = CompilerBrowser.FromText(Text);
            var thenToken = compiler.LocatePosition(Text.IndexOf("then"));
            var elseToken = compiler.LocatePosition(Text.IndexOf("else"));
            Tracer.Assert(elseToken.Syntax.LeftMostRightSibling == thenToken.Syntax);
        }

        [UnitTest]
        public void NestedMatching()
        {
            const string Text = @"1 then 2 then 333 else 3";
            var compiler = CompilerBrowser.FromText(Text);
            var thenToken = compiler.LocatePosition(Text.IndexOf("then"));
            var elseToken = compiler.LocatePosition(Text.IndexOf("else"));
            Tracer.Assert(elseToken.Syntax.LeftMostRightSibling == thenToken.Syntax);
        }
    }
}