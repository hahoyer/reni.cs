using System;
using System.Collections.Generic;
using System.Linq;
using hw.UnitTest;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.Reference
{
    [UnitTest]
    [ArrayElementType]
    [TargetSet(@"
text: 'abcdefghijklmnopqrstuvwxyz';
pointer: ((text type item)*1) array_reference instance (text);
pointer item(7) dump_print;
pointer item(0) dump_print;
pointer item(11) dump_print;
pointer item(11) dump_print;
pointer item(14) dump_print;
", "hallo")]
    public sealed class ArrayReferenceByInstance : CompilerTest {}
}