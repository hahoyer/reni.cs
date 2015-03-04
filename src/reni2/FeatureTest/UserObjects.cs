using System;
using System.Collections.Generic;
using System.Linq;
using hw.UnitTest;

namespace Reni.FeatureTest
{
    [TestFixture]
    [TargetSet(@"
system: /!\
{ MaxNumber8: /!\ '7f' to_number_of_base 16 
. MaxNumber16: /!\ '7fff' to_number_of_base 16 
. MaxNumber32: /!\ '7fffffff' to_number_of_base 16 
. MaxNumber64: /!\ '7fffffffffffffff' to_number_of_base 16 
};

o: /\ 
{
    re: system MaxNumber32 instance(^);
    im: system MaxNumber32 instance(0)
};

o (2) dump_print


", "(2,0)")]
    public class UserObjects : CompilerTest {}
}