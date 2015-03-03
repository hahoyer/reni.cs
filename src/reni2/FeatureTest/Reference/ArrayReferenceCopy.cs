using System;
using System.Collections.Generic;
using System.Linq;
using hw.UnitTest;

namespace Reni.FeatureTest.Reference
{
    [TestFixture]
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