using hw.DebugFormatter;
using hw.Scanner;
using hw.UnitTest;

namespace ReniUI.Test
{
    [UnitTest]
    public sealed class BadUserInterAction : DependenceProvider
    {
        const string Text = @"systemdata:
{ 1 type instance

#  #(aa  Memory: ((0 type * ('100' to_number_of_base 64)) mutable) instance();
   
   !mutable FreePointer: Memory array_reference mutable;
repeat: @ ^ while() then(^ body(), repeat(^));

  aa)#

}; 


(Text('H') << 'allo') dump_print";

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