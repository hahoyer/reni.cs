using hw.UnitTest;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.Function
{
    [UnitTest]
    [ObjectProperty]
    [Target(@"f: @(value: ^, x: @ ^);f(2) x(100) dump_print")]
    [Output("100")]
    public sealed class ObjectFunction2 : CompilerTest
    {
    }
}