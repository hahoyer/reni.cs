using hw.UnitTest;
using Reni.FeatureTest.Helper;
using Reni.FeatureTest.TypeType;

namespace Reni.FeatureTest.Array
{
    [UnitTest]
    [Target("((<<5<<3) << (<<5<<1<<3)) dump_print")]
    [Output("<<(5, 3, 5, 1, 3)")]
    [ArrayFromPieces]
    public sealed class CombineArraysFromPieces : CompilerTest;

    [UnitTest]
    [Target("((<<5<<3) << (<<5<<1<<3)) type dump_print")]
    [Output("((number(bits:4))!!!3)*5")]
    [ArrayFromPieces]
    [TypeOperator]
    public sealed class TypeOfCombineArraysFromPieces : CompilerTest;
}