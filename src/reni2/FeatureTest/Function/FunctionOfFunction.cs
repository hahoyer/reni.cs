using System.Linq;
using System.Collections.Generic;
using System;
using hw.UnitTest;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.Function
{
    [TestFixture]
    [TargetSet(@"f: /\ ^(); x: 1; f(/\x) dump_print", "1")]
    [Function]
    [TwoFunctions]
    [SimpleFunctionWithNonLocal]
    public sealed class FunctionOfFunction : CompilerTest {}

    [TestFixture]
    [FunctionOfFunction]
    [TargetSet(@"f: /\ ^ + 2; g: /\ ^(10); g(f)dump_print", "12")]
    public sealed class FunctionArgument : CompilerTest { }
}