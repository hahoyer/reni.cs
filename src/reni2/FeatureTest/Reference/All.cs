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
    count: ^ count
};

Text: /\
{ 
    !mutable data: ^ array_reference;
    _elementType: ^ type >>;
    _count: ^ count;
    AfterCopy: /\ NewMemory(elementType: _elementType, count: _count);
};

Text('Hallo') AfterCopy() result dump_print
")]
    [Output("((bit)*8[text_item])reference[force_mutable][mutable]")]
    public sealed class ArrayElementType1 : CompilerTest {}

    [TestFixture]
    [Target(@"
d: <<5<<2<<3;
ref: d array_reference;
(ref >> 0) dump_print;
(ref >> 1) dump_print;
(ref >> 2) dump_print;
")]
    [Output("523")]
    public sealed class ArrayReferenceCopy : CompilerTest {}

    [TestFixture]
    [Target(@"
d: <<5<<2<<:=3;
ref: d array_reference;
(d >> 0):= 1;
(ref >> 0) dump_print;
(ref >> 1) dump_print;
(ref >> 2) dump_print;
")]
    [Output("123")]
    public sealed class ArrayReferenceCopyAssign : CompilerTest {}

    [TestFixture]
    [Target(@"
o: 
/\
{ 
    data: ^ array_reference ;
    dump_print: 
    /!\ 
    {
        (data >> 0) dump_print;
        (data >> 1) dump_print;
        (data >> 2) dump_print;
        (data >> 3) dump_print;
        (data >> 4) dump_print;
        (data >> 5) dump_print
    }
};

o('abcdef') dump_print
")]
    [Output("abcdef")]
    public sealed class ArrayReferenceDumpSimple : CompilerTest {}

    [TestFixture]
    [ArrayReferenceDumpSimple]
    [Target(@"
repeat: /\ ^ while() then(^ body(), repeat(^));

o: 
/\
{ 
    data: ^ array_reference ;
    count: ^ count;
    dump_print: 
    /!\ 
    {
        !mutable position: count type instance (0) ;
        repeat
        (
            while: /\ position < count,
            body: /\ 
            ( 
                (data >> position) dump_print, 
                position := (position + 1) enable_cut
            ) 
        )
    }
};

o('abcdef') dump_print
")]
    [Output("abcdef")]
    public sealed class ArrayReferenceDumpLoop : CompilerTest {}

    [TestFixture]
    [ArrayReferenceCopyAssign]
    [ArrayReferenceCopy]
    [ArrayReferenceDumpLoop]
    public sealed class ArrayReferenceAll : CompilerTest {}
}