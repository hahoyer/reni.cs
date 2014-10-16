using System.Collections.Generic;
using System.Linq;
using System;
using hw.UnitTest;
using Reni.FeatureTest.Array;
using Reni.FeatureTest.BitArrayOp;

namespace Reni.FeatureTest.Text
{
    [TestFixture]
    [TargetSet(@"'Hallo' dump_print", "Hallo")]
    public sealed class Hallo : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [TargetSet(@"
x: 'Hallo';
x(0) dump_print;
x(1) dump_print;
x(2) dump_print;
x(3) dump_print;
x(4) dump_print;
", "Hallo")]
    [Hallo]
    [ElementAccessVariable]
    [ArrayVariable]
    public sealed class Hallo01234 : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [TargetSet("'Hal''lo' dump_print", "Hal'lo")]
    [Hallo]
    public sealed class HalloApo : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [TargetSet("\"Hal''lo\" dump_print", "Hal''lo")]
    [HalloApo]
    public sealed class HalloApoApo : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [TargetSet("\"Hal'\"\"'lo\" dump_print", "Hal'\"'lo")]
    [HalloApoApo]
    public sealed class HalloApoQuoApo : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [TargetSet("('Hallo' << ' Welt!') dump_print", "Hallo Welt!")]
    [Hallo]
    [CombineArraysFromPieces]
    public sealed class HalloWelt : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [Hallo]
    [TargetSet("108 text_item dump_print", "l")]
    public sealed class ConvertFromNumber : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [ConvertFromNumber]
    [TargetSet("(<< (108)text_item<< (109)text_item)text_item dump_print", "lm")]
    [ArrayFromPieces]
    [Hallo]
    public sealed class ConvertFromNumbers : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [Hallo]
    [Number]
    [ConvertFromNumber]
    [TargetSet("('80' to_number_of_base 16) dump_print", "128")]
    public sealed class ConvertHexadecimal : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }
}