using hw.UnitTest;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.Structure
{
    [UnitTest]
    [AccessSimple]
    [TargetSet(@"((0, 1) _A_T_ 1) dump_print;", "1")]
    public sealed class AccessSimple1 : CompilerTest
    {}
}