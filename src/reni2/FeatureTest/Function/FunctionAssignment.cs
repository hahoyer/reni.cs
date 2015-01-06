using System;
using System.Collections.Generic;
using System.Linq;
using hw.UnitTest;

namespace Reni.FeatureTest.Function
{
    [TestFixture]
    [SimpleFunctionWithNonLocal]
    [TargetSet(@"f: (^ + new_value)dump_print/\ ^; f(100) := 2;", "102")]
    public sealed class FunctionAssignment : CompilerTest {}
}