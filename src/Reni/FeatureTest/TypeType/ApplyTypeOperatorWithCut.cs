using hw.UnitTest;
using Reni.FeatureTest.ConversionService;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.TypeType;

[UnitTest]
[ApplyTypeOperator]
[NumberPointerCutConversion]
public sealed class ApplyTypeOperatorWithCut : CompilerTest
{
    protected override string Target => @"31 type instance (100 enable_cut) dump_print";
    protected override string Output => "-28";
}