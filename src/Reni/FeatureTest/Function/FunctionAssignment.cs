using hw.UnitTest;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.Function
{
    [UnitTest]
    [SimpleFunctionWithNonLocal]
    [TargetSet(@"f: (^ + new_value)dump_print@ ^; f(100) := 2;", "102")]
    public sealed class FunctionAssignment : CompilerTest {}
}