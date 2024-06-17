using hw.UnitTest;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.Array
{
    [UnitTest]
    [Target("(5 type * 5) instance () dump_print")]
    [Output("<<(0, 0, 0, 0, 0)")]
    public sealed class DefaultInitialized : CompilerTest;
}