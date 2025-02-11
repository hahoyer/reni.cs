using hw.UnitTest;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.Structure;

[UnitTest]
[TargetSet(@"
x1: 1;
x2: 4;
x3: 2050;
x4: x1 + x2 + x3;
x4 dump_print;
", "2055")]
[AccessAndAddComplex]
public sealed class SomeVariables : CompilerTest;