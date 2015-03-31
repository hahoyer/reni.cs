using System.Linq;
using System.Collections.Generic;
using System;
using hw.UnitTest;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.Structure
{
    [UnitTest]
    [InnerAccess]
    [TargetSet(@"(!mutable: 10, (^^ _A_T_ 0) := 4) dump_print", "(4, )")]
    public sealed class SimpleAssignment : CompilerTest
    {}
}