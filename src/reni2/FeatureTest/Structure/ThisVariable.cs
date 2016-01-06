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
  this: /!\ ^^ stable_reference;
  xxx: 12;
  result: this xxx; 
},

x()result dump_print

", "12")]
    public sealed class ThisVariable0 : CompilerTest {}

    [UnitTest]
    [Access]
    [TargetSet(@"
x: /\ 
{
  this: /!\ ^^ stable_reference;
  !mutable xxx: 12;
  result: this xxx; 
};

xx : x();
xx this xxx := 2;

x()result dump_print

", "2")]
    public sealed class ThisVariable1 : CompilerTest { }


    [UnitTest]
    [ThisVariable0]
    [ThisVariable1]
    [TargetSet(@"
x: /\ 
{
  this: /!\ ^^ stable_reference;
  xxx: 12;
  result: this xxx; 
} result ,

x() dump_print

", "12")]
    public sealed class ThisVariable : CompilerTest { }
}