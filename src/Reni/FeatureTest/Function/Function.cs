using hw.UnitTest;
using Reni.FeatureTest.Helper;
using Reni.FeatureTest.Structure;

namespace Reni.FeatureTest.Function;

[UnitTest]
[Target(@"f: @ ^;f(2) dump_print;")]
[Output("2")]
[InnerAccess]
[SomeVariables]
public sealed class Function : CompilerTest;