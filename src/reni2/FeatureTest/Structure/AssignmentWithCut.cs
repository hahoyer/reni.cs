using System.Linq;
using System.Collections.Generic;
using System;
using hw.UnitTest;
using Reni.FeatureTest.Helper;
using Reni.FeatureTest.TypeType;

namespace Reni.FeatureTest.Structure
{
    [UnitTest]
    [TargetSet(@"( !mutable:3, (^^ _A_T_ 0) := 5 enable_cut) dump_print", "(-3, )")]
    [Assignments]
    [ApplyTypeOperatorWithCut]
    public sealed class AssignmentWithCut : CompilerTest
    {}
}