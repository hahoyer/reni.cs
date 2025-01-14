using hw.UnitTest;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.TypeType;

[UnitTest]
[BitArrayOp.Number]
[Target("31 type dump_print")]
[Output("number(bits:6)")]
public sealed class TypeOperator : CompilerTest;