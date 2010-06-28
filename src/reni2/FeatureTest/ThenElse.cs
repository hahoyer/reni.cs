using System;
using HWClassLibrary.Debug;
using HWClassLibrary.UnitTest;

namespace Reni.FeatureTest.ThenElse
{
    [TestFixture, Target(@"x: 1=1 then 1 else 100;x dump_print;"), Output("1")]
    public class UseThen : CompilerTest
    {
        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }

    [TestFixture, Target(@"x: 1=0 then 1 else 100;x dump_print;"), Output("100")]
    public class UseElse : CompilerTest
    {
        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }
}