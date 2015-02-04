using System.Linq;
using System.Collections.Generic;
using System;
using hw.UnitTest;
using Reni.FeatureTest.ThenElse;

namespace Reni.FeatureTest.Function
{
    [TestFixture]
    [TargetSet(@"!mutable i: 400000 type instance(10); f: /\ i > 0 then (i := (i - 1)enable_cut; i dump_print; f());f()", "9876543210"
        )]
    [PrimitiveRecursiveFunctionByteWithDump]
    [UseThen]
    [UseElse]
    public sealed class PrimitiveRecursiveFunctionWithDump : CompilerTest
    {}
}