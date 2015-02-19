using System;
using System.Collections.Generic;
using System.Linq;
using hw.UnitTest;

namespace Reni.FeatureTest.Reference
{
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
}