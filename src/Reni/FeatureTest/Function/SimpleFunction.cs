using hw.UnitTest;
using Reni.FeatureTest.BitArrayOp;
using Reni.FeatureTest.Helper;
using Reni.FeatureTest.Structure;

namespace Reni.FeatureTest.Function;

[UnitTest]
[TargetSet(@"f: @ ^ + 1;f(2) dump_print;", "3")]
[InnerAccess]
[Add2Numbers]
[Function]
public sealed class SimpleFunction : CompilerTest;