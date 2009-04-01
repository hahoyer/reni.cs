using NUnit.Framework;

namespace Reni.FeatureTest.Array
{
    [TestFixture]
    [Target("(1,3,5,1) array")]
    [Output("3")]
    public class FromStruct : CompilerTest
    {
        public override void Run() { BaseRun(); }
    }
    [TestFixture]
    [Target("(function arg) array 5")]
    [Output("3")]
    public class FromFunction : CompilerTest
    {
        public override void Run() { BaseRun(); }
    }
}