using hw.UnitTest;
using reniUI.Test.Classification;

namespace ReniUI.Test.Classification;

[UnitTest]
public sealed class Comments : DependenceProvider
{
    [UnitTest]
    public void GetTokenForPosition()
    {
        const string text = @"x: 3;#";
        const string type = @"ikwnkc";

        text.GetTokenForPosition(type);
    }
}