using hw.UnitTest;
using Reni.FeatureTest.Helper;

// ReSharper disable once CheckNamespace
namespace Reni.FeatureTest.Reference
{
    [UnitTest]
    [Target("(<<5) type dump_print")]
    [Output("((number(bits:4))!!!3)*1")]
    public sealed class ReferenceSimple : CompilerTest;
}