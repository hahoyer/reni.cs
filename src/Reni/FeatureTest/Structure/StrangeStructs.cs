using hw.UnitTest;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.Structure;

[UnitTest]
[AccessSimple1]
[InnerAccess]
[AccessAndAdd]
[TargetSet(@"((one: 1) one) dump_print;", "1")]
[TargetSet(@"((one: 1,) one) dump_print;", "1")]
[TargetSet(@"((0,one: 1) one) dump_print;", "1")]
[TargetSet(@"((0,one: 1,) one) dump_print;", "1")]
public sealed class StrangeStructs : CompilerTest;