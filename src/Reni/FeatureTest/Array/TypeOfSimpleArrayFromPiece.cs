using hw.UnitTest;
using Reni.FeatureTest.Helper;
using Reni.FeatureTest.TypeType;

namespace Reni.FeatureTest.Array;

[UnitTest]
[Target("(<<5) type dump_print")]
[Output("((number(bits:4))!!!3)*1")]
[ParserTest.ParserTest]
[TypeOperator]
public sealed class TypeOfSimpleArrayFromPiece : CompilerTest;

[UnitTest]
[Target("(<<5)() type dump_print")]
[Output("number(bits:4)")]
[TypeOfSimpleArrayFromPiece]
public sealed class TypeOfElementOfSimpleArrayFromPiece : CompilerTest;