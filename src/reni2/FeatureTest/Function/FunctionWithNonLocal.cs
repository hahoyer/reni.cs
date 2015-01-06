using System.Linq;
using System.Collections.Generic;
using System;
using hw.UnitTest;
using Reni.FeatureTest.BitArrayOp;
using Reni.FeatureTest.Structure;

namespace Reni.FeatureTest.Function
{
    [TestFixture]
    [Target(@"x: 100;f: /\ ^ +x;f(2) dump_print;")]
    [Output("102")]
    [InnerAccess]
    [SomeVariables]
    [Add2Numbers]
    [SimpleFunctionWithNonLocal]
    public sealed class FunctionWithNonLocal : CompilerTest {}
}