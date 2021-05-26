using hw.UnitTest;
using Reni.FeatureTest.Helper;
using Reni.FeatureTest.Structure;

namespace Reni.FeatureTest.BlogExamples
{
    [UnitTest]
    [AccessSimple1]
    [TargetSet("\"Hello world\" dump_print", "Hello world")]
    [TargetSet(@"Viersich: 4;
EinsDazu: @ ^ + 1 ;
Konstrukt: 
@(
    Simpel: ^; 
    !mutable Pelsim: EinsDazu(^) ; 
    Fun: @ Simpel+ EinsDazu(^)
);
lorum: Konstrukt(23);
ipsum: Konstrukt(8);
ipsum Pelsim := 15;
(Viersich, ipsum Simpel, ipsum Pelsim, ipsum Fun(7), lorum Simpel, lorum Fun(18)) dump_print"
        , "(4, 8, 15, 16, 23, 42)")]
    public sealed class Post120617 : CompilerTest
    {}
}