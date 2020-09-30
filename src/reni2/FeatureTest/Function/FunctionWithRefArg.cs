using hw.UnitTest;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.Function
{
    [UnitTest]
    [Target(@"f: /\ ^;g: /\f(^);x:4; g(x)dump_print")]
    [Output("4")]
    [SimpleFunction]
    public sealed class FunctionWithRefArg : CompilerTest
    {
        [UnitTest]
        public override void Run() => BaseRun();
    }
}