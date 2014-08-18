using System.Linq;
using System.Collections.Generic;
using System;
using hw.UnitTest;

namespace Reni.FeatureTest.Structure
{
    [TestFixture]
    [SimpleAssignment]
    [TargetSet(@"(x: 10 enable_reassign, x := 4) dump_print", "(4, )")]
    public sealed class NamedSimpleAssignment : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }
}