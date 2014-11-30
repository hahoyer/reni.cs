using System.Linq;
using System.Collections.Generic;
using System;
using hw.UnitTest;

namespace Reni.FeatureTest.Structure
{
    [TestFixture]
    [TargetSet(@"(10, <:=>20,30, (^^ _A_T_ 1) := 4) dump_print", "(10, 4, 30, )")]
    [SimpleAssignment]
    public sealed class SimpleAssignment1 : CompilerTest
    {
    }
}