using System;
using System.Collections.Generic;
using System.Linq;
using hw.UnitTest;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.Reference
{
    [UnitTest]
    [Target(@"
d: <<5<<2<<3;
ref: d array_reference;
ref item( 0) dump_print;
ref item( 1) dump_print;
ref item( 2) dump_print;
")]
    [Output("523")]
    public sealed class ArrayReferenceCopy : CompilerTest {}
}