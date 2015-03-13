using System.Linq;
using System.Collections.Generic;
using System;
using hw.UnitTest;
using Reni.FeatureTest.Helper;
using Reni.FeatureTest.Structure;

namespace Reni.FeatureTest.Function
{
    [TestFixture]
    [FunctionWithNonLocal]
    [PropertyVariable]
    [Target(@"f: /\(value: ^, x: /!\value);f(2) x dump_print")]
    [Output("2")]
    public sealed class ObjectProperty : CompilerTest {}
}