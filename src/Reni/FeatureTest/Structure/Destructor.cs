using hw.UnitTest;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.Structure;

[UnitTest]
[TargetSet(@"
x: @ 
(
  '12345';
  this: @! ^^;
  '12345678901';
  !mutable xxx: 257;
  '12345678901234567890123456789';
~~~
'GetCleanup' dump_print
);

xx : x();
xx this xxx := 2;
xx dump_print

", "(12345, @!, 12345678901, 2, 12345678901234567890123456789)")]
public sealed class Destructor : CompilerTest;