using System.Linq;
using hw.Debug;
using hw.Scanner;
using hw.UnitTest;

namespace Reni.FeatureTest.UserInterface
{
    [UnitTest]
    public sealed class UserInterAction : DependantAttribute
    {
        const string text = @"
system: /!\
{ MaxNumber8: /!\ '7f' to_number_of_base 16 
. MaxNumber16: /!\ '7fff' to_number_of_base 16 
. MaxNumber32: /!\ '7fffffff' to_number_of_base 16 
. MaxNumber64: /!\ '7fffffffffffffff' to_number_of_base 16 
};

complex: 
{
    FromReal: /\ Create(^;0);

    Create: /\
    {
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
    }
};

complex FromReal(2) dump_print;
' ' dump_print;
(complex Create(0,1) * complex Create(0,1)) dump_print
";


        [UnitTest]
        public void TypingAProgram()
        {
            for(var i = 0; i < text.Length; i++)
            {
                var textFragement = text.Substring(0, i);
                var compiler = new Compiler(text: textFragement);
                var syntax = compiler.SourceSyntax;
                var span = syntax.SourcePart;
                Tracer.Assert(span.Id == textFragement, () => span.NodeDump);
            }
        }

        [UnitTest]
        public void GetTokenForPosition()
        {
            var compiler = new Compiler(text: text);
            for(var i = 0; i < text.Length; i++)
            {
                var t = compiler.Containing(i);
                Tracer.Assert(t != null, () => (new Source(text) + i).Dump());
            }
        }
    }
}