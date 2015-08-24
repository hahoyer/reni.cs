using System;
using System.Collections.Generic;
using System.Linq;
using hw.UnitTest;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.MixIn
{
    [UnitTest]
    [TargetSet(@"
Base: /\ 
{
    a: 3
}; 

This: /\ 
{
    !mix_in: Base()
}; 

This()a dump_print
", "3")]
    public sealed class Simple : CompilerTest {}
}