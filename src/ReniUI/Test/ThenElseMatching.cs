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
            (elseToken.Master.Left == thenToken.Master)
                .Assert
                (
                    () =>
                        "elseToken.Master.Left = " +
                        $"{elseToken.Master.Left.FlatItem.Anchor.d}\n\n" +
                        $"thenToken.Master = {thenToken.Master.FlatItem.Anchor.d}"
                );
        }

        [UnitTest]
        public void NestedMatching()
        {
            const string text = @"1 then 2 then 333 else 3";
            var compiler = CompilerBrowser.FromText(text);
            var thenToken = compiler.LocatePosition(text.IndexOf("then"));
            var elseToken = compiler.LocatePosition(text.IndexOf("else"));
            (elseToken.Master.Left == thenToken.Master).Assert();
        }
    }
}