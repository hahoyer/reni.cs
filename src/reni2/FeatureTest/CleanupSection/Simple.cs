using hw.UnitTest;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.CleanupSection
{
    [UnitTest]
    [Basic]
    [TargetSet(@"This: {(a:1;b:3) a; 5 ~~~'end' dump_print} dump_print ", "(1, 5)end")]
    public sealed class Simple : CompilerTest {}

    [UnitTest]
    [Simple]
    [TargetSet(@"This: /\ {a: 3;~~~ a dump_print}; This() dump_print", "(3)3")]
    public sealed class WithReference : CompilerTest { }
}