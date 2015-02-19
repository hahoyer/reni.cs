using System;
using System.Collections.Generic;
using System.Linq;
using hw.UnitTest;

namespace Reni.FeatureTest.Reference
{
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
}