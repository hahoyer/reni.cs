using System;
using System.Collections.Generic;
using System.Linq;
using hw.UnitTest;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.Reference
{
    [TestFixture]
    [Target(@"
NewMemory: /\ 
{ 
    result: (((^ elementType) * 1) array_reference mutable),
    count: ^ count
};

Text: /\
{ 
    !mutable data: ^ array_reference;
    _elementType: ^ type item;
    _count: ^ count;
    AfterCopy: /\ NewMemory(elementType: _elementType, count: _count);
};

Text('Hallo') AfterCopy() result dump_print
")]
    [Output("((bit)*8[text_item])reference[force_mutable][mutable]")]
    public sealed class ArrayElementType1 : CompilerTest {}
}