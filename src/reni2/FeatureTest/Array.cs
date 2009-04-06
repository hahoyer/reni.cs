using NUnit.Framework;

namespace Reni.FeatureTest.Array
{
    [TestFixture]
    [Target("(1,3,5,1) array dump_print")]
    [Output("array(4 bits,(1,3,5,1))")]
    public class FromStruct : CompilerTest
    {
        [Test, Category(UnderConstruction)]
        public override void Run() { BaseRun(); }
    }
    [TestFixture]
    [Target("(function arg) array(5) dump_print")]
    [Output("array(4 bits,(0,1,2,3,4))")]
    public class FromFunction : CompilerTest
    {
        [Test, Category(UnderConstruction)]
        public override void Run() { BaseRun(); }
    }
}