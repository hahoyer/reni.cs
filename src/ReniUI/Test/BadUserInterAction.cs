using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
using hw.UnitTest;

namespace ReniUI.Test
{
    [UnitTest]
    public sealed class BadUserInterAction : DependantAttribute
    {
        const string text = @"systemdata:
{ 1 type instance

#  #(aa  Memory: ((0 type * ('100' to_number_of_base 64)) mutable) instance();
   
   !mutable FreePointer: Memory array_reference mutable;
repeat: /\ ^ while() then(^ body(), repeat(^));

  aa)#

}; 


(Text('H') << 'allo') dump_print";

        [UnitTest]
        public void GetTokenForPosition()
        {
            var compiler = CompilerBrowser.FromText(text);
            for(var i = 0; i < text.Length; i++)
            {
                var t = compiler.LocatePosition(i);
                Tracer.Assert(t != null, () => (new Source(text) + i).Dump());
            }
        }
    }
}