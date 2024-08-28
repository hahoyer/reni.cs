using hw.DebugFormatter;
using hw.Helper;
using hw.UnitTest;
using Reni.Parser;

// ReSharper disable StringIndexOfIsCultureSpecific.1
// ReSharper disable StringIndexOfIsCultureSpecific.2

namespace ReniUI.Test;

[UnitTest]
public sealed class PairedSyntaxTree : DependenceProvider
{
    [UnitTest]
    public void SimpleList()
    {
        const string text = @"(1)";

        var compiler = CompilerBrowser.FromText(text);

        var open = compiler.GetToken(0);
        var close = compiler.GetToken(text.IndexOf(")"));

        var matchOpen = open.ParserLevelGroup;
        var matchClose = close.ParserLevelGroup;

        var pairs = matchOpen.Merge(matchClose, item => item).ToArray();

        (pairs.Length == 2).Assert(pairs.Dump);
    }

    [UnitTest]
    public void TopLevelList()
    {
        const string text = @"1,3";

        var compiler = CompilerBrowser.FromText(text);

        var list = compiler.GetToken(text.IndexOf(","));

        var matchList = list.ParserLevelGroup.ToArray();
        (matchList.Length == 1).Assert();
        (matchList.DumpSource() == "1[,]3").Assert();
    }

    [UnitTest]
    public void List()
    {
        const string text = @"(1,3)";

        var compiler = CompilerBrowser.FromText(text);

        var open = compiler.GetToken(0);
        var close = compiler.GetToken(text.IndexOf(")"));

        var matchOpen = open.ParserLevelGroup;
        var matchClose = close.ParserLevelGroup;

        var pairs = matchOpen.Merge(matchClose, item => item).ToArray();

        (pairs.Length == 2).Assert();
    }

    [UnitTest]
    public void List3()
    {
        const string text = @"(1,2,3)";

        var compiler = CompilerBrowser.FromText(text);

        var open = compiler.GetToken(0);
        var close = compiler.GetToken(text.IndexOf(")"));

        var matchOpen = open.ParserLevelGroup;
        var matchClose = close.ParserLevelGroup;

        var pairs = matchOpen.Merge(matchClose, item => item).ToArray();

        (pairs.Length == 2).Assert();
    }
}