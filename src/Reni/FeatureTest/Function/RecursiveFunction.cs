using hw.UnitTest;
using Reni.FeatureTest.BitArrayOp;
using Reni.FeatureTest.DefaultOperations;
using Reni.FeatureTest.Helper;
using Reni.FeatureTest.Structure;
using Reni.FeatureTest.ThenElse;
using Reni.FeatureTest.TypeType;

namespace Reni.FeatureTest.Function
{
    [UnitTest]
    [InnerAccess]
    [Add2Numbers]
    [UseThen]
    [UseElse]
    [ApplyTypeOperator]
    [Equal]
    [ApplyTypeOperatorWithCut]
    [SimpleFunction]
    [TwoFunctions1]
    [FunctionWithRefArg]
    [Target(@"
f: @
{
    1000 type instance
    (
        {
            ^ = 1 
            then 1 
            else ^ * f[^ type instance((^ - 1)enable_cut)]
        }
        enable_cut
    )
};
f(4)dump_print"
        )]
    [Output("24")]
    public sealed class RecursiveFunction : CompilerTest
    {
    }
}