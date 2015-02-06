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
t: a type >>;
t dump_print
")]
    [Output("(bit)*8[text_item]")]
    public sealed class ArrayElementType : CompilerTest {}


    [TestFixture]
    [Target(@"
NewMemory: /\ 
{ 
    result: (((^ elementType) * 1) array_reference mutable),
    length: ^ length
};

Text: /\
{ 
    !mutable data: ^ array_reference;
    _elementType: ^ type >>;
    _length: ^ length;
    AfterCopy: /\ NewMemory(elementType: _elementType, length: _length);
};

Text('Hallo') AfterCopy() result dump_print
")]
    [Output("(((bit)*8[text_item])*1)reference")]
    public sealed class ArrayElementType1 : CompilerTest {}
}