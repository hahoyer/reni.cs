using hw.UnitTest;
using Reni.FeatureTest.Helper;
using Reni.FeatureTest.Structure;

namespace Reni.FeatureTest.Function
{
    [UnitTest]
    [FunctionWithNonLocal]
    [PropertyVariable]
    [Target(@"f: /\(value: ^, x: /!\value);f(2) x dump_print")]
    [Output("2")]
    public sealed class ObjectProperty : CompilerTest {}
}