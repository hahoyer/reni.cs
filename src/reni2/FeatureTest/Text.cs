using System.Collections.Generic;
using System.Linq;
using System;
using hw.Parser;
using hw.UnitTest;
using Reni.FeatureTest.Array;
using Reni.FeatureTest.BitArrayOp;

namespace Reni.FeatureTest.Text
{
    [TestFixture]
    [TargetSet(@"'Hallo' dump_print", "Hallo")]
    public sealed class Hallo : CompilerTest
    {}

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
    {}

    [TestFixture]
    [TargetSet("'Hal''lo' dump_print", "Hal'lo")]
    [Hallo]
    public sealed class HalloApo : CompilerTest
    {}

    [TestFixture]
    [TargetSet("\"Hal''lo\" dump_print", "Hal''lo")]
    [HalloApo]
    public sealed class HalloApoApo : CompilerTest
    {}

    [TestFixture]
    [TargetSet("\"Hal'\"\"'lo\" dump_print", "Hal'\"'lo")]
    [HalloApoApo]
    public sealed class HalloApoQuoApo : CompilerTest
    {}

    [TestFixture]
    [TargetSet("('Hallo' << ' Welt!') dump_print", "Hallo Welt!")]
    [Hallo]
    [CombineArraysFromPieces]
    public sealed class HalloWelt : CompilerTest
    {}

    [TestFixture]
    [Hallo]
    [TargetSet("108 text_item dump_print", "l")]
    public sealed class ConvertFromNumber : CompilerTest
    {}

    [TestFixture]
    [ConvertFromNumber]
    //[CompilerParameters.Trace.Parser]
    [TargetSet("(<< (108)text_item<< (109)text_item)text_item dump_print", "lm")]
    [ArrayFromPieces]
    [Hallo]
    public sealed class ConvertFromNumbers : CompilerTest
    {}

    [TestFixture]
    [Hallo]
    [Number]
    [ConvertFromNumber]
    [ConversionService.Closure]
    [TargetSet("('80' to_number_of_base 16) dump_print", "128")]
    public sealed class ConvertHexadecimal : CompilerTest
    {}
}