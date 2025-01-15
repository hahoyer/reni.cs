using hw.UnitTest;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.Function;

[UnitTest]
[SimpleFunctionWithNonLocal]
[TwoFunctions1]
public sealed class TwoFunctions : CompilerTest
{
    protected override string Target => @"
x: 100;
f1: @((
  y: 3;
  f: @ ^ * y + x;
  f(2)
) _A_T_ 2);

f1()dump_print;
";

    protected override string Output => "106";
}