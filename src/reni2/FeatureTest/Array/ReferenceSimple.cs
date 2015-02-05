using System;
using System.Collections.Generic;
using System.Linq;
using hw.UnitTest;

namespace Reni.FeatureTest.Reference
{
    [TestFixture]
    [Target("(<<5) type dump_print")]
    [Output("((number(bits:4))!!!3)*1")]
    public sealed class ReferenceSimple : CompilerTest
    { }
}