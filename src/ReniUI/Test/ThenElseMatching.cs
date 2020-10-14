using hw.DebugFormatter;
using hw.UnitTest;
// ReSharper disable StringIndexOfIsCultureSpecific.1

namespace ReniUI.Test
{
    [UnitTest]
    public sealed class ThenElseMatching : DependenceProvider
    {
        [UnitTest]
        public void Matching()
        {
            const string text = @"1 then 2 else 3";
            var compiler = CompilerBrowser.FromText(text);
            var thenToken = compiler.LocatePosition(text.IndexOf("then"));
            var elseToken = compiler.LocatePosition(text.IndexOf("else"));
            Tracer.Assert(elseToken.Master.Left == thenToken.Master,
                ()=>$"elseToken.Master.Left = {elseToken.Master.Left.FlatItem.d}\n\n" +
                    $"thenToken.Master = {thenToken.Master.FlatItem.d}"
                );
        }

        [UnitTest]
        public void NestedMatching()
        {
            const string text = @"1 then 2 then 333 else 3";
            var compiler = CompilerBrowser.FromText(text);
            var thenToken = compiler.LocatePosition(text.IndexOf("then"));
            var elseToken = compiler.LocatePosition(text.IndexOf("else"));
            Tracer.Assert(elseToken.Master.Left == thenToken.Master);
        }
    }
}