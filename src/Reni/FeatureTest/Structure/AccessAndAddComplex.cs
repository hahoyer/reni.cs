using hw.UnitTest;
using Reni.FeatureTest.BitArrayOp;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.Structure;

[UnitTest]
[AccessAndAdd]
[BitArrayOp.BitArrayOp]
[SubtractOddSizedNumber]
[TargetSet(" 1; 4;2050; (_A_T_ 0) + (_A_T_ 1) + (_A_T_ 2);(_A_T_ 3) dump_print;", "2055")]
public sealed class AccessAndAddComplex : CompilerTest;