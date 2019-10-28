using System;
using System.Collections.Generic;
using System.Linq;
using hw.UnitTest;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.CleanupSection
{
    [UnitTest]
    [Simple]
    [TargetSet(@"
Base: /\ 
{
    a: 1;
~~~
'(base cleanup)' dump_print
}; 

This: /\ 
{
    !mix_in: Base();
    a: 3;
~~~
'(this cleanup)' dump_print
}; 

This() dump_print
", "((1, ), 3, )(this cleanup)(base cleanup)")]
    public sealed class Nested : CompilerTest {}

}