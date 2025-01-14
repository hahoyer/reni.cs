using hw.UnitTest;
using Reni.FeatureTest.Helper;
using Reni.FeatureTest.Reference;

namespace Reni.FeatureTest.MixIn;

[UnitTest]
[ReferenceSimple]
[ArrayReferenceAll]
[TargetSet(@"
Base: @ 
(
    a: 3;
); 

This: @ 
(
    !mix_in: #(  )# Base();
    aa: 2;
); 

This()a dump_print
", "3")]
public sealed class Simple : CompilerTest;