using hw.DebugFormatter;
using hw.Scanner;
using hw.UnitTest;
using Reni;

namespace ReniUI.Test
{
    [UnitTest]
    public sealed class UserInterAction : DependenceProvider
    {
        const string Text = @"
System: 
(
\!
{ MaxNumber8: \! '7f' to_number_of_base 16 
. MaxNumber16: \! '7fff' to_number_of_base 16 
. MaxNumber32: \! '7fffffff' to_number_of_base 16 
. MaxNumber64: \! '7fffffffffffffff' to_number_of_base 16 
};

complex: 
{
    FromReal: \ Create(^;0);

    Create: \
    {
        re: system MaxNumber8 type instance((^_A_T_ 0) enable_cut);
        im: system MaxNumber8 type instance((^_A_T_ 1) enable_cut);
        + : \ complex Create(re + ^re, im + ^im);
        * : \ complex Create(re * ^re - im * ^im, re * ^im + im * ^re);
        dump_print: \!
        (
            re dump_print;
            '+' dump_print;
            im dump_print;
            'i' dump_print
        )                                                          \
    }
};

complex FromReal(2) dump_print;
' ' dump_print;
(complex Create(0,1) * complex Create(0,1)) dump_print
";


        [UnitTest]
        public void TypingAProgram()
        {
            for(var i = 0; i < Text.Length; i++)
            {
                var textFragment = Text.Substring(0, i);
                var compiler = Compiler.FromText(textFragment);
                var syntax = compiler.Syntax;
                var span = syntax.Anchor.SourcePart;
                (span.Id == textFragment).Assert(() => span.NodeDump);
            }
        }

        [UnitTest]
        public void GetTokenForPosition()
        {
            var compiler = CompilerBrowser.FromText(Text);
            for(var i = 0; i < Text.Length; i++)
            {
                var t = compiler.LocatePosition(i);
                (t != null).Assert(() => (new Source(Text) + i).Dump());
            }
        }
    }
}