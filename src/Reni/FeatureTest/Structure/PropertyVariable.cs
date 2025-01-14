using hw.UnitTest;
using Reni.FeatureTest.Function;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.Structure;

[UnitTest]
[InnerAccess]
[Function.Function]
[SimpleFunctionWithNonLocal]
[FunctionAssignment]
public sealed class PropertyVariable : CompilerTest
{
    protected override string Target => @"x: @!11; x dump_print";
    protected override string Output => "11";

    [UnitTest]
    public override void Run() => BaseRun();
}