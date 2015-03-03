using System;
using System.Collections.Generic;
using System.Linq;
using hw.UnitTest;

namespace Reni.FeatureTest.Reference
{
    [TestFixture]
    [ArrayElementType1]
    [Target(@"
a: 'Text';
t: a type item;
t dump_print
")]
    [Output("(bit)*8[text_item]")]
    public sealed class ArrayElementType : CompilerTest {}
}