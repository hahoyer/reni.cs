using hw.UnitTest;

namespace ReniUI.Test.Formatting;

[Lists]
[UnitTest]
public sealed class TrainWreck : DependenceProvider
{
    [UnitTest]
    public void FunctionWithTrainWreck()
    {
        const string text = @"Text: @
{
    value: ^.
}
result
";
        text.SimpleFormattingTest();
    }

    [UnitTest]
    public void TwoWagons()
    {
        const string text = @"{
    first.

    system
        NewMemory.

    second.
}";
        text.SimpleFormattingTest();
    }

    [UnitTest]
    public void TreeWagons()
    {
        const string text = @"{
    first.

    system
        NewMemory
        Attribute.

    second.
}";
        text.SimpleFormattingTest();
    }

    [UnitTest]
    public void ComplexHead()
    {
        const string text = @"{
    first.

    (system parameters)
        NewMemory.

    second.
}";
        text.SimpleFormattingTest();
    }

}