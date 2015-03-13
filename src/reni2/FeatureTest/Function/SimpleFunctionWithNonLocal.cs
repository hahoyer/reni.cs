using System.Linq;
using System.Collections.Generic;
using System;
using hw.UnitTest;
using Reni.FeatureTest.Helper;
using Reni.FeatureTest.Structure;

namespace Reni.FeatureTest.Function
{
    [TestFixture]
    [Target(@"x: 100; f: /\x;f() dump_print;")]
    [Output("100")]
    [InnerAccess]
    [SomeVariables]
    [ConstantFunction]
    [SimpleFunction]
    public sealed class SimpleFunctionWithNonLocal : CompilerTest {}
}