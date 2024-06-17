using hw.UnitTest;
using Reni.FeatureTest.Array;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.Reference
{
    [UnitTest]
    [Target(@"
NewMemory: @ 
( 
    result: (((^ elementType) * 1) array_reference mutable),
    count: ^ count
);

Text: @
( 
    !mutable data: ^ array_reference;
    _elementType: ^() type;
    _count: ^ count;
    AfterCopy: @ NewMemory(elementType: _elementType, count: _count);
);

Text('Hallo') AfterCopy() result dump_print
")]
    [Output("((bit)*8[text_item])reference[force_mutable][mutable]")]
    [TypeOfElementOfSimpleArrayFromPiece]
    public sealed class ArrayElementType1 : CompilerTest;
}