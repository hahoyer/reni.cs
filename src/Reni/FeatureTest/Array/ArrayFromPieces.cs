using hw.UnitTest;
using Reni.FeatureTest.Helper;
using Reni.FeatureTest.TypeType;

namespace Reni.FeatureTest.Array;

[UnitTest]
[SimpleArrayFromPiece]
[Target("(<<5<<3<<5<<1) dump_print")]
[Output("<<(5, 3, 5, 1)")]
public sealed class ArrayFromPieces : CompilerTest;

[UnitTest]
[TypeOfSimpleArrayFromPiece]
[TypeOperator]
[Target("(<<5<<3<<5<<1) type dump_print")]
[Output("((number(bits:4))!!!3)*4")]
public sealed class TypeOfArrayFromPieces : CompilerTest;