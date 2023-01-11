using hw.Helper;
using hw.UnitTest;
using Reni.FeatureTest.Helper;

namespace ReniUI.Generated.At230110_010030;

[UnitTest]
public class Test : CompilerTest
{
    protected override string Target => "Text.reni".ToSmbFile().String;
}