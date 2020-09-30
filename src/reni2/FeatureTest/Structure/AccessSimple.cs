using hw.UnitTest;
using Reni.FeatureTest.ConversionService;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.Structure
{
    [UnitTest]
    [Closure]
    [NumberExtensionConversion]
    [TargetSet(@"((0, 1) _A_T_ 0) dump_print", "0")]
    [TargetSet(@"((0, 1, ) _A_T_ 0) dump_print", "0")]
    public sealed class AccessSimple : CompilerTest {}

    [UnitTest]
    [TargetSet(@"1;1 dump_print", "1")]
    public sealed class TwoStatements : CompilerTest {}
}