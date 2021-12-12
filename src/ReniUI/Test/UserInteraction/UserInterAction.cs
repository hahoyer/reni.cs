using hw.DebugFormatter;
using hw.Scanner;
using hw.UnitTest;
using Reni;
using ReniUI.Test.Classification;

namespace ReniUI.Test.UserInteraction
{
    [UnitTest]
    [All]
    public sealed class UserInterAction : DependenceProvider
    {
        readonly string Text = @"
System: 
(
@!
{ MaxNumber8: @! '7f' to_number_of_base 16 
. MaxNumber16: @! '7fff' to_number_of_base 16 
. MaxNumber32: @! '7fffffff' to_number_of_base 16 
. MaxNumber64: @! '7fffffffffffffff' to_number_of_base 16 
};

complex: 
{
    FromReal: @ Create(^;0);

    Create: @
    {
        re: system MaxNumber8 type instance((^_A_T_ 0) enable_cut);
        im: system MaxNumber8 type instance((^_A_T_ 1) enable_cut);
        + : @ complex Create(re + ^re, im + ^im);
        * : @ complex Create(re * ^re - im * ^im, re * ^im + im * ^re);
        dump_print: @!
        (
            re dump_print;
            '+' dump_print;
            im dump_print;
            'i' dump_print
        )                                                          @
    }
};

complex FromReal(2) dump_print;
' ' dump_print;
(complex Create(0,1) * complex Create(0,1)) dump_print
".Replace("@r@n", "@n");


        [UnitTest]
        public void TypingAProgram()
        {
            for(var i = 0; i < Text.Length; i++)
            {
                var textFragment = Text.Substring(0, i);
                var compiler = Compiler.FromText(textFragment);
                var syntax = compiler.Syntax;
                syntax.AssertIsNotNull();
            }
        }

        [UnitTest]
        public void GetTokenForPosition()
        {
            var compiler = CompilerBrowser.FromText(Text);
            for(var offset = 0; offset < Text.Length; offset++)
            {
                var t = compiler.Locate(offset);
                (t != null).Assert(() => (new Source(Text) + offset).Dump());
            }
        }
    }
}