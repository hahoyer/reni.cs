using System.Linq;
using System.Collections.Generic;
using System;
using hw.UnitTest;

namespace Reni.FeatureTest.Structure
{
    [TestFixture]
    [AccessSimple]
    [TargetSet(@"((0, 1) _A_T_ 1) dump_print;", "1")]
    public sealed class AccessSimple1 : CompilerTest
    {}
}