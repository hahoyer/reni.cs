using System;
using System.Collections.Generic;
using System.Linq;
using hw.UnitTest;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.Structure
{
    [UnitTest]
    [InnerAccess]
    [TargetSet(@"
x: /\ 
{
  256;
  this: ^^;
  xxx: 257;
  258
},

x() dump_print

", "12")]
    public sealed class ContextOperator0 : CompilerTest {}

    [UnitTest]
    [Access]
    [NamedSimpleAssignment]
    [TargetSet(@"
x: /\ 
{
  this: ^^ ;
  !mutable xxx: 12 
};

xx : x();
xx this xxx := 2;
xx dump_print

", "2")]
    public sealed class ContextOperator1 : CompilerTest { }


    [UnitTest]
    [ContextOperator0]
    [ContextOperator1]
    [TargetSet(@"
x: /\ 
{
  this: /!\ ^^ stable_reference;
  xxx: 12;
  result: this xxx; 
} result ,

x() dump_print

", "12")]
    public sealed class ContextOperator : CompilerTest { }
}