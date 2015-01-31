using System.Linq;
using System.Collections.Generic;
using System;
using hw.UnitTest;

namespace Reni.FeatureTest.Function
{
    [TestFixture]
    [Target(@"f: /\100;f() dump_print;")]
    [Output("100")]
    [Function]
    public sealed class ConstantFunction : CompilerTest {}
}