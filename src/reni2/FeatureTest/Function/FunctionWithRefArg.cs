using System.Linq;
using System.Collections.Generic;
using System;
using hw.UnitTest;

namespace Reni.FeatureTest.Function
{
    [TestFixture]
    [Target(@"f: /\ ^;g: /\f(^);x:4; g(x)dump_print")]
    [Output("4")]
    [SimpleFunction]
    public sealed class FunctionWithRefArg : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }
}