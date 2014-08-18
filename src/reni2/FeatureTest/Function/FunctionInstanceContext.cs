using System.Linq;
using System.Collections.Generic;
using System;
using hw.UnitTest;
using Reni.FeatureTest.BitArrayOp;
using Reni.FeatureTest.Structure;

namespace Reni.FeatureTest.Function
{
    [LowPriority]
    [TestFixture]
    [TargetSet(@"a:(x: 100;f: /\ . + x); a f(1) function_instance ^ dump_print;", @"(100, /\(.)+(x))")]
    [SomeVariables]
    [Add2Numbers]
    [AccessMember]
    [FunctionWithNonLocal]
    [Function]
    [TwoFunctions]
    [FunctionWithRefArg]
    public sealed class FunctionInstanceContext : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }
}