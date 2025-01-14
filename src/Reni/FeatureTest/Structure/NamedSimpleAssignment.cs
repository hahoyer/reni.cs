using hw.UnitTest;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.Structure;

[UnitTest]
[SimpleAssignment]
[TargetSet(@"(x!mutable : 10 , x := 4) dump_print", "(4, )")]
public sealed class NamedSimpleAssignment : CompilerTest;