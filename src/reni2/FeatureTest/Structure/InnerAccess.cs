using hw.UnitTest;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.Structure
{
    [UnitTest]
    [BitArrayOp.Number]
    [AccessSimple1]
    [TargetSet("5, (_A_T_ 0) dump_print, 66", "5")]
    [TargetSet("5,6, (_A_T_ 0) dump_print, 66", "5")]
    [TargetSet("5,6, (_A_T_ 1) dump_print, 66", "6")]
    public sealed class InnerAccess : CompilerTest
    {}
}