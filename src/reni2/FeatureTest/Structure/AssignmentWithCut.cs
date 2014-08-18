using System.Linq;
using System.Collections.Generic;
using System;
using hw.UnitTest;
using Reni.FeatureTest.TypeType;

namespace Reni.FeatureTest.Structure
{
    [TestFixture]
    [TargetSet(@"(3 enable_reassign, (^ _A_T_ 0) := 5 enable_cut) dump_print", "(-3, )")]
    [Assignments]
    [ApplyTypeOperatorWithCut]
    public sealed class AssignmentWithCut : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }
}