using System.Linq;
using System.Collections.Generic;
using System;
using hw.UnitTest;
using Reni.FeatureTest.BitArrayOp;
using Reni.FeatureTest.Structure;

namespace Reni.FeatureTest.Function
{
    [TestFixture]
    [TargetSet(@"f: /\ . + 1;f(2) dump_print;", "3")]
    [InnerAccess]
    [Add2Numbers]
    [Function]
    public sealed class SimpleFunction : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }
}