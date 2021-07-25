using hw.DebugFormatter;
using hw.UnitTest;
using Reni.Parser;

// ReSharper disable StringIndexOfIsCultureSpecific.1

namespace ReniUI.Test
{
    [UnitTest,Classification.All]
    public sealed class ThenElseMatching : DependenceProvider
    {
        [UnitTest]
        public void Matching()
        {
            const string text = @"1 then 2 else 3";
            var compiler = CompilerBrowser.FromText(text);
            var thenToken = compiler.Locate(text.IndexOf("then"));
            var elseToken = compiler.Locate(text.IndexOf("else"));
            (elseToken.Master == thenToken.Master)
                .Assert
                (
                    () =>
                        "elseToken.Master= " +
                        $"{elseToken.Master.Anchor.SourceParts.DumpSource()}\n\n" +
                        $"thenToken.Master = {thenToken.Master.Anchor.SourceParts.DumpSource()}"
                );
            (elseToken.Index == 1).Assert();
            (thenToken.Index == 0).Assert();
        }

        [UnitTest]
        public void NestedMatching()
        {
            const string text = @"1 then 2 then 333 else 3";
            var compiler = CompilerBrowser.FromText(text);
            var thenToken = compiler.Locate(text.IndexOf("then"));
            var elseToken = compiler.Locate(text.IndexOf("else"));
            (elseToken.Master == thenToken.Master).Assert();
            (elseToken.Index == 1).Assert();
            (thenToken.Index == 0).Assert();
        }
    }
}