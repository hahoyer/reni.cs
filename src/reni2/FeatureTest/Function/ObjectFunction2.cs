using System.Linq;
using System.Collections.Generic;
using System;
using hw.UnitTest;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.Function
{
    [TestFixture]
    [ObjectProperty]
    [Target(@"f: /\(value: ^, x: /\ ^);f(2) x(100) dump_print")]
    [Output("100")]
    public sealed class ObjectFunction2 : CompilerTest
    {
    }
}