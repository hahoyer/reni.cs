using System.Linq;
using System.Collections.Generic;
using System;
using hw.UnitTest;

namespace Reni.FeatureTest.Function
{
    [TestFixture]
    [ObjectProperty]
    [Target(@"f: /\(value: ^, x: /\value);f(2) x(100) dump_print")]
    [Output("2")]
    public sealed class ObjectFunction1 : CompilerTest {}
}