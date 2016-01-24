using System;
using System.Collections.Generic;
using System.Linq;
using hw.UnitTest;
using Reni.FeatureTest.Array;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.Reference
{
    [UnitTest]
    [ElementAccessVariableSetterSimple]
    [Target(@"
d: << 5 << 2 <<:= 3;
ref: d array_reference;
d(0) := 1;
ref(0) dump_print;
ref(1) dump_print;
ref(2) dump_print;
")]
    [Output("123")]
    public sealed class ArrayReferenceCopyAssign : CompilerTest {}
}