using hw.UnitTest;
using Reni.FeatureTest.Helper;
using Reni.FeatureTest.Structure;

namespace Reni.FeatureTest.CleanupSection
{
    [UnitTest]
    [Basic]
    [AccessMember]
    [TargetSet(@"This: {(aaaaa:1;b:3) aaaaa; 5 ~~~'end' dump_print} dump_print ", "(1, 5)end")]
    public sealed class Simple : CompilerTest { }

    [UnitTest]
    [Simple]
    [TargetSet(@"This: /\ {a: 3;~~~ a dump_print}; This() dump_print", "(3)3")]
    public sealed class WithReference : CompilerTest { }
}