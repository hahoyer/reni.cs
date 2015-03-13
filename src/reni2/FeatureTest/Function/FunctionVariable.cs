using System.Linq;
using System.Collections.Generic;
using System;
using hw.UnitTest;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.Function
{
    [TestFixture]
    [SimpleFunction]
    [TargetSet(@"f: /\ ^;x: f; x(4)dump_print", "4")]
    public sealed class FunctionVariable : CompilerTest {}

}