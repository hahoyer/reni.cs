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

", "(256, ^^, 257, 258)")]
    public sealed class ContextOperatorPrint : CompilerTest {}

    [UnitTest]
    [Access]
    [NamedSimpleAssignment]
    [TargetSet(@"
x: /\ 
{
  256;
  this: ^^;
  xxx: 257;
  258
};

xx : x();
xx this xxx dump_print

", "257")]
    public sealed class ContextOperatorAccess : CompilerTest { }

    [UnitTest]
    [Access]
    [NamedSimpleAssignment, ContextOperatorAccess]
    [TargetSet(@"
x: /\ 
{
  256;
  this: ^^;
  !mutable xxx: 257;
  258
};

xx : x();
xx this xxx := 2;
xx dump_print

", "(256, ^^, 2, 258)")]
    public sealed class ContextOperatorAssign : CompilerTest { }


    [UnitTest]
    [ContextOperatorPrint, ContextOperatorAssign]
    [ContextOperatorAccess]
    [TargetSet(@"
x: /\ 
{
  256;
  this: /!\ ^^;
  !mutable xxx: 257;
  258
  result: this xxx; 
} result ,

x() dump_print

", "257")]
    public sealed class ContextOperator : CompilerTest { }
}