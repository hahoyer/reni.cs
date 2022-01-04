using hw.DebugFormatter;
using hw.Scanner;
using hw.UnitTest;

namespace ReniUI.Test.UserInteraction
{
    [UnitTest]
    [PairedSyntaxTree]
    [UserInterAction]
    public sealed class BadUserInterAction : DependenceProvider
    {
        [UnitTest]
        public void GetTokenForPosition()
        {
            const string text = @"systemdata:
{ 1 type instance

#  #(aa  Memory: ((0 type * ('100' to_number_of_base 64)) mutable) instance();
   
   !mutable FreePointer: Memory array_reference mutable;
repeat: @ ^ while() then(^ body(), repeat(^));

  aa)#

}; 


(Text('H') << 'allo') dump_print";

            var compiler = CompilerBrowser.FromText(text);
            for(var i = 0; i < text.Length; i++)
            {
                var t = compiler.Locate(i);
                (t != null).Assert(() => (new Source(text) + i).Dump());
            }
        }

        [UnitTest]
        public void GetTokenForPositionSimple()
        {
            const string text = @"
{ 1 }";
            var compiler = CompilerBrowser.FromText(text);
            for(var i = 0; i < text.Length; i++)
            {
                var t = compiler.Locate(i);
                (t != null).Assert(() => (new Source(text) + i).Dump());
            }

        }
    }
}