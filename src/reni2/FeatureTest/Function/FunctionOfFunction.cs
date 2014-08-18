using System.Linq;
using System.Collections.Generic;
using System;
using hw.UnitTest;

namespace Reni.FeatureTest.Function
{
    [TestFixture]
    [TargetSet(@"f: /\ .(); x: 1; f(/\x) dump_print", "1")]
    [Function]
    [TwoFunctions]
    [SimpleFunctionWithNonLocal]
    public sealed class FunctionOfFunction : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }
}