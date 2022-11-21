using hw.UnitTest;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.Array;

[UnitTest]
[Target(@"((100 type) array_instance (4,8,15,16,23,42)) dump_print")]
[Output("<<(4, 8, 15, 16, 23, 42)")]
[ParserTest.ParserTest]
public sealed class ArrayFromList : CompilerTest { }