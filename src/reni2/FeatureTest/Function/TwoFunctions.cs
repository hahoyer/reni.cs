using System.Linq;
using System.Collections.Generic;
using System;
using hw.UnitTest;

namespace Reni.FeatureTest.Function
{
    [TestFixture]
    [SimpleFunctionWithNonLocal]
    [TwoFunctions1]
    public sealed class TwoFunctions : CompilerTest
    {
        protected override string Target
        {
            get
            {
                return
                    @"
x: 100;
f1: /\((
  y: 3;
  f: /\ ^ * y + x;
  f(2)
) _A_T_ 2);

f1()dump_print;
";
            }
        }

        protected override string Output { get { return "106"; } }
    }
}