using System;
using System.Collections.Generic;
using System.Linq;
using hw.UnitTest;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.Structure
{
    [UnitTest]
    [Access]
    [TargetSet(@"
x: /\ 
{
  this: /!\ ^^;
  xxx: 12;
  result: this xxx; 
},

x()result dump_print

", "12")]
    public sealed class ThisVariable : CompilerTest {}
}