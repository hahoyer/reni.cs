using System;
using System.Collections.Generic;
using System.Linq;
using hw.UnitTest;
using Reni.Parser;

namespace Reni.FeatureTest.TypeType
{
    [TestFixture]
    [PrioTableTest]
    public sealed class TypeOperator : CompilerTest
    {
        protected override string Target { get { return @"31 type dump_print"; } }
        protected override string Output { get { return "number(6)"; } }

        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [TypeOperator]
    [TargetSet("(0 type * 3) sequence dump_print", "(#(#align3#)# (bit)sequence(1))sequence(3)")]
    public sealed class SequenceOfType : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }
}