using hw.UnitTest;
using Reni.FeatureTest.Array;
using Reni.FeatureTest.Helper;

// ReSharper disable once CheckNamespace
namespace Reni.FeatureTest.Text;

[UnitTest]
[TargetSet(@"'Hallo' dump_print", "Hallo")]
public sealed class Hallo : CompilerTest;

[UnitTest]
[TargetSet(@"
x: 'Hallo';
x (0) dump_print;
x (1) dump_print;
x (2) dump_print;
x (3) dump_print;
x (4) dump_print;
", "Hallo")]
[Hallo]
[ElementAccessVariable]
[ArrayVariable]
public sealed class Hallo01234 : CompilerTest;

[UnitTest]
[TargetSet("'Hal''lo' dump_print", "Hal'lo")]
[Hallo]
public sealed class HalloApo : CompilerTest;

[UnitTest]
[TargetSet("\"Hal''lo\" dump_print", "Hal''lo")]
[HalloApo]
public sealed class HalloApoApo : CompilerTest;

[UnitTest]
[TargetSet("\"Hal'\"\"'lo\" dump_print", "Hal'\"'lo")]
[HalloApoApo]
public sealed class HalloApoQuoApo : CompilerTest;

[UnitTest]
[TargetSet("('Hallo' << ' Welt!') dump_print", "Hallo Welt!")]
[Hallo]
[CombineArraysFromPieces]
public sealed class HalloWelt : CompilerTest;

[UnitTest]
[Hallo]
[TargetSet("108 text_item dump_print", "l")]
public sealed class ConvertFromNumber : CompilerTest;

[UnitTest]
[ConvertFromNumber]
//[CompilerParameters.Trace.Parser]
[TargetSet("(<< (108)text_item<< (109)text_item)text_item dump_print", "lm")]
[ArrayFromPieces]
[Hallo]
public sealed class ConvertFromNumbers : CompilerTest;

[UnitTest]
[Hallo]
[BitArrayOp.Number]
[ConvertFromNumber]
[ConversionService.Closure]
[TargetSet("('80' to_number_of_base 16) dump_print", "128")]
public sealed class ConvertHexadecimal : CompilerTest;