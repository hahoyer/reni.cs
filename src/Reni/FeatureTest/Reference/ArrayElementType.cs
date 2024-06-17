using hw.UnitTest;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.Reference
{
    [UnitTest]
    [ArrayElementType1]
    [Target(@"
a: 'Text';
t: a() type;
t dump_print
")]
    [Output("(bit)*8[text_item]")]
    public sealed class ArrayElementType : CompilerTest;
}