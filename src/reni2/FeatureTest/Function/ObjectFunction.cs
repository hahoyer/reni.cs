using System.Linq;
using System.Collections.Generic;
using System;
using hw.UnitTest;

namespace Reni.FeatureTest.Function
{
    [TestFixture]
    [ObjectFunction1]
    [ObjectFunction2]
    [Target(@"f: /\(value: ^, x: /\ ^ + value);f(2) x(100) dump_print")]
    [Output("102")]
    public sealed class ObjectFunction : CompilerTest {}
}