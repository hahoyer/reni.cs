using hw.UnitTest;
using ReniUI.Formatting;

namespace ReniUI.Test.Formatting;

[UnitTest]
public sealed class Basics : DependenceProvider
{
    [UnitTest]
    public void BreakLine() => @"aaaaa bbbbb".SimpleFormattingTest(@"aaaaa
bbbbb", 10);

    [UnitTest]
    public void One() => @"aaaaa".SimpleFormattingTest();

    [UnitTest]
    public void Two() => @"aaaaa bbbbb".SimpleFormattingTest();

    [UnitTest]
    public void BreakLine3() => @"aaaaa bbbbb ccccc".SimpleFormattingTest(@"aaaaa
bbbbb
ccccc", 10);

    [UnitTest]
    public void BreakLineWithLimit1() => @"aaaaa 

bbbbb".SimpleFormattingTest(@"aaaaa
bbbbb",
        emptyLineLimit: 1);

    [UnitTest]
    public void BreakLineWithLimit0() => @"aaaaa 

bbbbb".SimpleFormattingTest(@"aaaaa bbbbb",
        emptyLineLimit: 0);


    [UnitTest]
    public void OmitSpaceWhenLineBreakRemains()
    {
        const string text =
            @"a
b";

        var expectedText = @"a
b"
            .Replace("\r\n", "\n");

        text.SimpleFormattingTest(expectedText);
    }

    [UnitTest]
    public void UseSpaceWhenLineBreakIsRemoved()
    {
        const string text =
            @"a
b";

        var expectedText = @"a b".Replace("\r\n", "\n");

        var compiler = CompilerBrowser.FromText(text);
        var newSource = compiler.Reformat
            (
                new ReniUI.Formatting.Configuration
                    { EmptyLineLimit = 0 }.Create()
            )
            .Replace("\r\n", "\n");


        (newSource == expectedText).Assert("\n\"" + newSource + "\"");
    }

    [UnitTest]
    public void EmptyBrackets()
    {
        const string text =
            @"a()
b";

        var expectedText = @"a() b".Replace("\r\n", "\n");

        var compiler = CompilerBrowser.FromText(text);
        var newSource = compiler.Reformat
            (
                new ReniUI.Formatting.Configuration
                    { EmptyLineLimit = 0 }.Create()
            )
            .Replace("\r\n", "\n");

        (newSource == expectedText).Assert("\n\"" + newSource + "\"");
    }

    [UnitTest]
    public void Reformat()
    {
        const string text = @"
^ body(),
            repeat(^),
";
        const string expectedText = @"
^ body(),
repeat(^),
";

        text.SimpleFormattingTest(expectedText, 120, 1);
    }
}