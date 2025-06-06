using hw.UnitTest;

namespace ReniUI.Test.Formatting;

[UnitTest]
public sealed class FlatStrings : DependenceProvider
{
    [UnitTest]
    public void AddSeparators()
    {
        const string text = @"(1,3,4,6)";
        var compiler = CompilerBrowser.FromText(text);
        var flatString = compiler.Compiler.BinaryTree.GetFlatString(false)!;

        (flatString == "(1, 3, 4, 6)").Assert(flatString);
    }

    [UnitTest]
    public void AddSeparators1()
    {
        const string text = @"aaaaa;bbbbb;ccccc;";
        var compiler = CompilerBrowser.FromText(text);
        var flatString = compiler.Compiler.BinaryTree.GetFlatString(false)!;

        (flatString == "aaaaa; bbbbb; ccccc;").Assert(flatString);
        (flatString.Length >= 20).Assert(flatString);
    }
}