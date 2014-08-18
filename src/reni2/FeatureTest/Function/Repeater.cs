using System.Linq;
using System.Collections.Generic;
using System;
using hw.UnitTest;
using Reni.FeatureTest.DefaultOperations;
using Reni.FeatureTest.Structure;
using Reni.FeatureTest.ThenElse;
using Reni.FeatureTest.TypeType;

namespace Reni.FeatureTest.Function
{
    [TestFixture]
    [Target(@"
repeat: /\ .() while then repeat(.);

count: 10,
index: (count type instance(0)) enable_reassign,
repeat
(/\(
    while: index < count, 
    while then
    (
        index dump_print, 
        ' ' dump_print, 
        index := (index + 1)enable_cut
    )
))
")]
    [Output("0 1 2 3 4 5 6 7 8 9 ")]
    [Equal]
    [Assignments]
    [TypeOperator]
    [BitArrayOp.BitArrayOp]
    [UseThen]
    [FunctionOfFunction]
    public sealed class SimpleRepeater : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [Target(@"
repeat: /\ . while() then(. body(), repeat(.));

count: 10,
index: (count type instance(0)) enable_reassign,
repeat
(
    while: /\ index < count, 
    body: /\
    (
        index dump_print, 
        ' ' dump_print, 
        index := (index + 1)enable_cut
    )
)
")]
    [Output("0 1 2 3 4 5 6 7 8 9 ")]
    [SimpleRepeater]
    public sealed class Repeater : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }
}