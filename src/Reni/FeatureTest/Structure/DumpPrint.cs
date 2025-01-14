using hw.UnitTest;
using Reni.FeatureTest.ConversionService;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.Structure;

[UnitTest]
[Target("(1) dump_print")]
[Output("1")]
[Closure]
public sealed class List1 : CompilerTest;

[UnitTest]
[Target("(1, 2) dump_print")]
[Output("(1, 2)")]
[List1]
public sealed class List2 : CompilerTest;

[UnitTest]
[Target("(1, 2, 3) dump_print")]
[Output("(1, 2, 3)")]
[List2]
public sealed class List3 : CompilerTest;

[UnitTest]
[Target("(1, 2,) dump_print")]
[Output("(1, 2)")]
[List3]
public sealed class List2AndEmpty : CompilerTest;

[UnitTest]
[Target("(1, 2, 3, 4, 5, 6) dump_print")]
[Output("(1, 2, 3, 4, 5, 6)")]
[List2AndEmpty]
public sealed class DumpPrint : CompilerTest;