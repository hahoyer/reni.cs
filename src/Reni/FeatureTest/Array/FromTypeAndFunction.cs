using hw.UnitTest;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.Array;

[UnitTest]
[Target(@"(5 type * 5) instance (@ ^) dump_print")]
[Output("<<(0, 1, 2, 3, 4)")]
[DefaultInitialized]
public sealed class FromTypeAndFunction : CompilerTest;