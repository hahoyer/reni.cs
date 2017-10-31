using hw.UnitTest;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest
{
    [UnitTest]
    [AllScopeHandling]
    [ComplexContext]
    [TargetSet
        (
            @"
system: /!\
( MaxNumber8: /!\ '7f' to_number_of_base 16 
. MaxNumber16: /!\ '7fff' to_number_of_base 16 
. MaxNumber32: /!\ '7fffffff' to_number_of_base 16 
. MaxNumber64: /!\ '7fffffffffffffff' to_number_of_base 16 
);

complex: 
(
    FromReal: /\ Create(^;0);

    Create: /\
    (
        re: system MaxNumber8 type instance((^_A_T_ 0) enable_cut);
        im: system MaxNumber8 type instance((^_A_T_ 1) enable_cut);
        + : /\ complex Create(re + ^re, im + ^im);
        * : /\ complex Create(re * ^re - im * ^im, re * ^im + im * ^re);
        dump_print: /!\
        (
            re dump_print;
            '+' dump_print;
            im dump_print;
            'i' dump_print
        )
    )
);

complex FromReal(2) dump_print;
' ' dump_print;
(complex Create(0,1) * complex Create(0,1)) dump_print
",
            "2+0i -1+0i")]
    public sealed class UserObjects : CompilerTest {}
}