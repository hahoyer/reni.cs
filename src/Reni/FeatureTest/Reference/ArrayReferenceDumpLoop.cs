using hw.UnitTest;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.Reference
{
    [UnitTest]
    [ArrayReferenceDumpSimple]
    [Target(@"
repeat: @ ^ while() then(^ body(), repeat(^));

o: 
@
( 
    data: ^ array_reference ;
    count: ^ count;
    dump_print: 
    @! 
    (
        !mutable position: count type instance (0) ;
        repeat
        (
            while: @ position < count,
            body: @ 
            ( 
                data (position) dump_print, 
                position := (position + 1) enable_cut
            ) 
        )
    )
);

o('abcdef') dump_print
")]
    [Output("abcdef")]
    public sealed class ArrayReferenceDumpLoop : CompilerTest;
}