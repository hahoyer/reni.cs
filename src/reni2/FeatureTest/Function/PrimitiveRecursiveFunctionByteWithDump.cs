using System.Linq;
using System.Collections.Generic;
using System;
using hw.UnitTest;
using Reni.FeatureTest.BitArrayOp;
using Reni.FeatureTest.Structure;
using Reni.FeatureTest.ThenElse;

namespace Reni.FeatureTest.Function
{
    [TestFixture]
    [InnerAccess]
    [Add2Numbers]
    [UseThen]
    [UseElse]
    [Assignments]
    [SimpleFunctionWithNonLocal]
    [RecursiveFunction]
    [NamedSimpleAssignment]
    [Target(@"!mutable i: 10; f: /\ i > 0 then (i := (i - 1)enable_cut; i dump_print; f());f()")]
    [Output("9876543210")]
    public sealed class PrimitiveRecursiveFunctionByteWithDump : CompilerTest
    {}
}