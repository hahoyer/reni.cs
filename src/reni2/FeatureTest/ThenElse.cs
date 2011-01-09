using System;
using HWClassLibrary.Debug;
using HWClassLibrary.UnitTest;
using Reni.FeatureTest.Struct;

namespace Reni.FeatureTest.ThenElse
{
    [TestFixture, Target(@"x: 1=1 then 1 else 100;x dump_print;"), Output("1"), InnerAccess]
    public sealed class UseThen : CompilerTest
    {
        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }

    [TestFixture, Target(@"x: 1=0 then 1 else 100;x dump_print;"), Output("100"), InnerAccess]
    public sealed class UseElse : CompilerTest
    {
        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }
}