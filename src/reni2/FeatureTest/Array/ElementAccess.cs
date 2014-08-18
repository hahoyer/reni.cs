using System.Linq;
using System.Collections.Generic;
using System;
using hw.UnitTest;

namespace Reni.FeatureTest.Array
{
    [TestFixture]
    [ArrayFromPieces]
    [TargetSet("((<<5) (0)) dump_print", "5")]
    public sealed class ElementAccessSimple : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [ElementAccessSimple]
    [TargetSet("((<<5<<3<<5<<1<<3) (3)) dump_print", "1")]
    public sealed class ElementAccess : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [ArrayFromPieces]
    [TargetSet("x: <<5<<3; x dump_print", "array(#(#align3#)# (bit)sequence(4),(5, 3))")]
    [LowPriority]
    public sealed class ArrayVariable : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [ElementAccessVariable]
    [ArrayVariable]
    [TargetSet("x: (<<5) enable_reassign; x(0) := 2; x dump_print", "array(#(#align3#)# (bit)sequence(4),(2))")]
    public sealed class ElementAccessVariableSetterSimple : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [ElementAccessVariableSetterSimple]
    [TargetSet("x: (<<5<<3<<5<<1<<3) enable_reassign; x(3) := 2; x dump_print", "array(#(#align3#)# (bit)sequence(4),(5, 3, 5, 2, 3))")]
    public sealed class ElementAccessVariableSetter : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [ElementAccess]
    [TargetSet("x: <<5<<3<<5<<1<<3; x(3) dump_print", "1")]
    public sealed class ElementAccessVariable : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }
}