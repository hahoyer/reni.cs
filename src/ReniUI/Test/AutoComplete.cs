using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
using hw.UnitTest;
using Reni;

namespace ReniUI.Test
{
    [UnitTest]
    [AutoCompleteFunctionInCompound]
    public sealed class AutoComplete : DependenceProvider
    {
        const string text = @"systemdata: 
{
    Memory: ((0 type *(125)) mutable) instance();
    !mutable FreePointer: Memory array_reference mutable;
};


repeat: @ ^ while  () then(^ body(), repeat(^));

system:
{
    MaxNumber8: @! '7f' to_number_of_base 16.
    MaxNumber16: @! '7fff' to_number_of_base 16.
    MaxNumber32: @! '7fffffff' to_number_of_base 16.
    MaxNumber64: @! '7fffffffffffffff' to_number_of_base 16.
    TextItemType: @! MaxNumber8 text_item type.

    NewMemory: @
    {

        result: (((^ elementType) * 1) array_reference mutable)
        instance(systemdata FreePointer enable_reinterpretation).

        initializer: ^ initializer.

        count: ^ count.
        !mutable position: count type instance(0).

        repeat
        (
            while: @ position < count,

            body: @
            (
                result(position) := initializer(position),
                position :=(position + 1) enable_cut
            )
        ).

        systemdata FreePointer :=(systemdata FreePointer type)
        instance((result + count) mutable enable_reinterpretation)
    }
    result
};
system MaxNumber8 +          ;
";

        [UnitTest]
        public void GetDeclarationOptions()
        {
            var compiler = CompilerBrowser.FromText(text);
            for(var offset = 1; offset < text.Length; offset++)
            {
                var position = compiler.Source + offset;
                if((position + -1).Span(2).Id != "\r\n")
                {
                    var t = compiler.DeclarationOptions(offset);
                    Tracer.Assert(t != null, () => (new Source(text) + offset).Dump());
                }
            }
        }
    }
}