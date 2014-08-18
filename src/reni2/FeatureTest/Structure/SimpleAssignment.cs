using System.Linq;
using System.Collections.Generic;
using System;
using hw.UnitTest;

namespace Reni.FeatureTest.Structure
{
    [TestFixture]
    [InnerAccess]
    [TargetSet(@"(10 enable_reassign, (^ _A_T_ 0) := 4) dump_print", "(4, )")]
    public sealed class SimpleAssignment : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }
}