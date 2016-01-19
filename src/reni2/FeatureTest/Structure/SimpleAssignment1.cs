using System.Linq;
using System.Collections.Generic;
using System;
using hw.UnitTest;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.Structure
{
    [UnitTest]
    [TargetSet(@"(10, !mutable: 20,30, (_A_T_ 1) := 4) dump_print", "(10, 4, 30, )")]
    [SimpleAssignment]
    public sealed class SimpleAssignment1 : CompilerTest
    {
    }
}