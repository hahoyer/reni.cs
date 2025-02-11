using hw.UnitTest;
using Reni.FeatureTest.Helper;
using Reni.FeatureTest.Structure;

namespace Reni.FeatureTest.BlogExamples;

[UnitTest]
[AccessSimple1]
[TargetSet(@"x: (1, 2); (x; 2) dump_print", "((1, 2), 2)")]
public sealed class NestedCompound : CompilerTest;

[UnitTest]
[AccessSimple1]
[NestedCompound]
[TargetSet(@"x: (4, 8, 15, 16, 23, 42); ('Die Lottozahlen lauten: '; x; 'Wir gratulieren den Gewinnern.') dump_print",
    "(Die Lottozahlen lauten: , (4, 8, 15, 16, 23, 42), Wir gratulieren den Gewinnern.)")]
public sealed class WikiExamples : CompilerTest;