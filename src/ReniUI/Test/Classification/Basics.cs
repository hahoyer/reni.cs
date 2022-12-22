using hw.UnitTest;
using reniUI.Test.Classification;

namespace ReniUI.Test.Classification;

[UnitTest]
public sealed class Basics : DependenceProvider
{
    [UnitTest]
    public void GetTokenForPosition()
    {
        const string text = @"   !mutable FreePointer: Memory array_reference mutable;";
        const string type = @"wwwkkkkkkkkwiiiiiiiiiiikwiiiiiiwiiiiiiiiiiiiiiiwiiiiiiik";

        text.GetTokenForPosition(type);
    }
}