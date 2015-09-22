using System;
using System.Collections.Generic;
using System.Linq;
using hw.UnitTest;
using Reni.FeatureTest.Helper;
using Reni.FeatureTest.Reference;

namespace Reni.FeatureTest.MixIn
{
    [UnitTest]
    [ReferenceSimple]
    [Simple]
    [ArrayReferenceAll]
    [TargetSet(@"
This: /\ 
{
    !mix_in: (dump_print(^),dump_print(new_value))/\ 100 type instance(^ enable_cut);
    aa: 2;
}; 

This() (1) := 3
", "3")]
    public sealed class Function : CompilerTest {}
}