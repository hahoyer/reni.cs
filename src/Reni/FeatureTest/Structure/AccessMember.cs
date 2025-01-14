using hw.UnitTest;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.Structure;

[UnitTest]
[TargetSet(@"a: (a: 11, b:222, c:4, d: 2722); a b dump_print", "222")]
[TargetSet(@"a: (a: 11, b:222, c:4, d: 2722); a c dump_print", "4")]
[InnerAccess]
public sealed class AccessMember : CompilerTest;