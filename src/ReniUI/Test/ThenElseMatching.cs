using hw.DebugFormatter;
using hw.UnitTest;
using Reni.Parser;

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
            (elseToken.Master == thenToken.Master)
                .Assert
                (
                    () =>
                        "elseToken.Master= " +
                        $"{elseToken.Master.FlatItem.Anchor.SourceParts.DumpSource()}\n\n" +
                        $"thenToken.Master = {thenToken.Master.FlatItem.Anchor.SourceParts.DumpSource()}"
                );
            (elseToken.Index == 2).Assert();
            (thenToken.Index == 1).Assert();
        }

        [UnitTest]
        public void NestedMatching()
        {
            const string text = @"1 then 2 then 333 else 3";
            var compiler = CompilerBrowser.FromText(text);
            var thenToken = compiler.LocatePosition(text.IndexOf("then"));
            var elseToken = compiler.LocatePosition(text.IndexOf("else"));
            (elseToken.Master == thenToken.Master).Assert();
            (elseToken.Index == 2).Assert();
            (thenToken.Index == 1).Assert();
        }
    }
}