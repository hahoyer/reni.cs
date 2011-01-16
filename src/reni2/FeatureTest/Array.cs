using HWClassLibrary.UnitTest;

namespace Reni.FeatureTest.Array
{
    [TestFixture]
    [Target("(()<*5<*3<*5<*1) dump_print")]
    [Output("array(4 bits,(5,3,5,1))")]
    public sealed class ArrayFromPieces : CompilerTest
    {
        [Test, Category(UnderConstruction),Explicit]
        public override void Run() { BaseRun(); }
    }
    [TestFixture]
    [Target("(()<*5<*3)<<(()<*5<*1) dump_print")]
    [Output("array(4 bits,(5,3,5,1))")]
    public sealed class CombineArraysFromPieces : CompilerTest
    {
        [Test, Category(UnderConstruction), Explicit]
        public override void Run() { BaseRun(); }
    }
    [TestFixture]
    [Target("(5 type * 5)(arg/\\) array dump_print")]
    [Output("array(4 bits,(0,1,2,3,4))")]
    public sealed class FromTypeAndFunction : CompilerTest
    {
        [Test, Category(UnderConstruction), Explicit]
        public override void Run() { BaseRun(); }
    }
}