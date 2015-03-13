using System.Linq;
using System.Collections.Generic;
using System;
using hw.UnitTest;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.Function
{
    [TestFixture]
    [Target(@"
f1: /\((
  y: 3;
  f: /\y;
  f(2)
) _A_T_ 2);

f1()dump_print;
")]
    [Output("3")]
    [SimpleFunctionWithNonLocal]
    [ObjectFunction]
    public sealed class TwoFunctions1 : CompilerTest {}
}