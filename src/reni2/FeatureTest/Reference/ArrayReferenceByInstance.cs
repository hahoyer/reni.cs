using hw.UnitTest;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.Reference
{
    [UnitTest]
    [ArrayElementType]
    [ArrayReferenceDumpSimple]
    [TargetSet(@"
text: 'abcdefghijklmnopqrstuvwxyz';
pointer: ((text() type)*1) array_reference instance (text);
pointer(7) dump_print;
pointer(0) dump_print;
pointer(11) dump_print;
pointer(11) dump_print;
pointer(14) dump_print;
", "hallo")]
    public sealed class ArrayReferenceByInstance : CompilerTest {}
}