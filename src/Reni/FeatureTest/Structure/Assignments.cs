using hw.UnitTest;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.Structure;

[UnitTest]
[TargetSet(@"(!mutable:10, !mutable:20, (_A_T_ 0) := 4) dump_print", "(4, 20, )")]
[TargetSet(@"(!mutable:10, !mutable:20, !mutable:30, (_A_T_ 0) := 4) dump_print", "(4, 20, 30, )")]
[TargetSet(@"(!mutable:10, !mutable:20, (_A_T_ 1) := 4) dump_print", "(10, 4, )")]
[TargetSet(@"(!mutable:10, !mutable:20, !mutable:30, (_A_T_ 2) := 4) dump_print", "(10, 20, 4, )")]
[TargetSet(@"(!mutable:1000, !mutable:20, !mutable:30, (_A_T_ 0) := 4) dump_print", "(4, 20, 30, )")]
[TargetSet(@"(!mutable:1000, !mutable:20, !mutable:30, (_A_T_ 1) := 4) dump_print", "(1000, 4, 30, )")]
[TargetSet(@"(!mutable:10, !mutable:2000, !mutable:30, (_A_T_ 0) := 4) dump_print", "(4, 2000, 30, )")]
[TargetSet(@"(!mutable:10, !mutable:2000, !mutable:30, (_A_T_ 1) := 4) dump_print", "(10, 4, 30, )")]
[TargetSet(@"(!mutable:10, !mutable:2000, !mutable:30, (_A_T_ 2) := 4) dump_print", "(10, 2000, 4, )")]
[SimpleAssignment]
[SimpleAssignment1]
public sealed class Assignments : CompilerTest;