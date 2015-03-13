using System.Linq;
using System.Collections.Generic;
using System;
using hw.UnitTest;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.Structure
{
    [TestFixture]
    [SimpleAssignment]
    [TargetSet(@"(!mutable x: 10 , x := 4) dump_print", "(4, )")]
    public sealed class NamedSimpleAssignment : CompilerTest
    {}
}