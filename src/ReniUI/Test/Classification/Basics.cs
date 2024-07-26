using hw.UnitTest;
using reniUI.Test.Classification;

namespace ReniUI.Test.Classification;

[UnitTest]
public sealed class Basics : DependenceProvider
{
    [UnitTest]
    public void GetTokenForPosition()
    {
        const string text = @"   FreePointer!mutable: Memory array_reference mutable;";
        const string type = @"wwwiiiiiiiiiiikkkkkkkkkwiiiiiiwiiiiiiiiiiiiiiiwiiiiiiik";

        text.GetTokenForPosition(type);
    }
}