using System;
using System.Collections.Generic;
using System.Linq;
using hw.UnitTest;

namespace Reni.FeatureTest.Reference
{
    [TestFixture]
    [ArrayElementType]
    [TargetSet(@"
text: 'abcdefghijklmnopqrstuvwxyz';
pointer: ((text type >>)*1) array_reference instance (text);
(pointer >> 7) dump_print;
(pointer >> 0) dump_print;
(pointer >> 11) dump_print;
(pointer >> 11) dump_print;
(pointer >> 14) dump_print;
", "hallo")]
    public sealed class ArrayReferenceByInstance : CompilerTest {}
}