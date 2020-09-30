using hw.UnitTest;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.Function
{
    [UnitTest]
    [ObjectFunction1]
    [ObjectFunction2]
    [Target(@"f: /\(value: ^, x: /\ ^ + value);f(2) x(100) dump_print")]
    [Output("102")]
    public sealed class ObjectFunction : CompilerTest {}
}