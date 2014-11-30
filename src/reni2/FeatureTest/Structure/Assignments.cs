using System.Linq;
using System.Collections.Generic;
using System;
using hw.UnitTest;

namespace Reni.FeatureTest.Structure
{
    [TestFixture]
    [TargetSet(@"(<:=>10, <:=>20, (^^ _A_T_ 0) := 4) dump_print", "(4, 20, )")]
    [TargetSet(@"(<:=>10, <:=>20, <:=>30 enable_reassign, (^^ _A_T_ 0) := 4) dump_print", "(4, 20, 30, )")]
    [TargetSet(@"(<:=>10, <:=>20, (^^ _A_T_ 1) := 4) dump_print", "(10, 4, )")]
    [TargetSet(@"(<:=>10, <:=>20, <:=>30, (^^ _A_T_ 2) := 4) dump_print", "(10, 20, 4, )")]
    [TargetSet(@"(<:=>1000, <:=>20, <:=>30, (^^ _A_T_ 0) := 4) dump_print", "(4, 20, 30, )")]
    [TargetSet(@"(<:=>1000, <:=>20, <:=>30, (^^ _A_T_ 1) := 4) dump_print", "(1000, 4, 30, )")]
    [TargetSet(@"(<:=>10, <:=>2000, <:=>30, (^^ _A_T_ 0) := 4) dump_print", "(4, 2000, 30, )")]
    [TargetSet(@"(<:=>10, <:=>2000, <:=>30, (^^ _A_T_ 1) := 4) dump_print", "(10, 4, 30, )")]
    [TargetSet(@"(<:=>10, <:=>2000, <:=>30, (^^ _A_T_ 2) := 4) dump_print", "(10, 2000, 4, )")]
    [SimpleAssignment]
    [SimpleAssignment1]
    public sealed class Assignments : CompilerTest
    {}
}