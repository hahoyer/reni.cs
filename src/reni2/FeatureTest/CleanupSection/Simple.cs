using System;
using System.Collections.Generic;
using System.Linq;
using hw.UnitTest;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.CleanupSection
{
    [UnitTest]
    [Basic]
    [TargetSet(@"
Base: /\ 
{
    a: 1;
~~~
'(base cleanup)' dump_print
}; 

This: /\ 
{
    !mixin: Base();
    a: 3;
~~~
'(this cleanup)' dump_print
}; 

This() dump_print
", "(1, 3)(base cleanup)(this cleanup)")]
    public sealed class Simple : CompilerTest {}
}