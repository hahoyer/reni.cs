using hw.UnitTest;
using Reni.FeatureTest.ConversionService;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.TypeType
{
    [UnitTest]
    [Closure, TypeOperator]
    public sealed class ApplyTypeOperator : CompilerTest
    {
        protected override string Target => @"(31 type instance (28))dump_print";
        protected override string Output => "28";
    }
}