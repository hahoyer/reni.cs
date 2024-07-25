using hw.UnitTest;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest;

[UnitTest]
[Target(@"((0 type *(125)) mutable) instance()")]
[Output("")]
public sealed class MutableExpression : CompilerTest;

[UnitTest]
[MutableExpression]
public sealed class Annotated : CompilerTest;