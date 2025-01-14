using hw.UnitTest;
using Reni.FeatureTest.BitArrayOp;
using Reni.FeatureTest.Helper;
using Reni.FeatureTest.Structure;

namespace Reni.FeatureTest.Function;

[UnitTest]
[Target(@"x: 100;f: @ ^ +x;f(2) dump_print;")]
[Output("102")]
[InnerAccess]
[SomeVariables]
[Add2Numbers]
[SimpleFunctionWithNonLocal]
public sealed class FunctionWithNonLocal : CompilerTest;