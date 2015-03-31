using System.Linq;
using System.Collections.Generic;
using System;
using hw.UnitTest;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.Function
{
    [UnitTest]
    [Target(@"f: /\100;f() dump_print;")]
    [Output("100")]
    [Function]
    public sealed class ConstantFunction : CompilerTest {}
}