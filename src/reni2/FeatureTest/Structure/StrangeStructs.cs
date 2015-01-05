using System.Linq;
using System.Collections.Generic;
using System;
using hw.UnitTest;

namespace Reni.FeatureTest.Structure
{
    [TestFixture]
    [AccessSimple1]
    [InnerAccess]
    [AccessAndAdd]
    [TargetSet(@"((one: 1) one) dump_print;", "1")]
    [TargetSet(@"((one: 1,) one) dump_print;", "1")]
    [TargetSet(@"((0,one: 1) one) dump_print;", "1")]
    [TargetSet(@"((0,one: 1,) one) dump_print;", "1")]
    public sealed class StrangeStructs : CompilerTest {}
}