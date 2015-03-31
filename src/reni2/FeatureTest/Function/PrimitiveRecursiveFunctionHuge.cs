using System.Linq;
using System.Collections.Generic;
using System;
using hw.UnitTest;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.Function
{
    [UnitTest]
    [Target(@"!mutable i: 400000; f: /\i > 0 then (i := (i - 1)enable_cut; f());f()")]
    [Output("")]
    [PrimitiveRecursiveFunctionSmall]
    //[LowPriority]
    public sealed class PrimitiveRecursiveFunctionHuge : CompilerTest
    {
    }
}