using System.Linq;
using System.Collections.Generic;
using System;
using hw.UnitTest;

namespace Reni.FeatureTest.Structure
{
    [TestFixture]
    [InnerAccess]
    [TargetSet(@"(<:=> 10, (^^ _A_T_ 0) := 4) dump_print", "(4, )")]
    public sealed class SimpleAssignment : CompilerTest
    {}
}