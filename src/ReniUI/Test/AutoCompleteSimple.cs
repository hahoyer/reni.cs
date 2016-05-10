using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
using hw.UnitTest;

namespace ReniUI.Test
{
    [UnitTest]
    public sealed class AutoCompleteSimple : DependantAttribute
    {

        const string text = @"

    NewMemory: /\
    {

        result: (((^ elementType) * 1) array_reference mutable)
        instance(systemdata FreePointer enable_reinterpretation).

        initializer: ^ initializer.

        count: ^ count.
        !mutable position: count type instance(0).

        repeat
        (
            while: /\ position < count,

            body: /\
            (
                result(position) := initializer(position),
                position :=(position + 1) enable_cut
            )
        ).

        systemdata FreePointer :=(systemdata FreePointer type)
        instance((result + count) mutable enable_reinterpretation)
    }
    result
";

        [UnitTest]
        public void GetDeclarationOptions()
        {
            var compiler = CompilerBrowser.FromText(text);
            for (var i = 0; i < text.Length; i++)
            {
                var offset = text.Length - i - 1;
                var position = compiler.Source + offset;
                if ((position + -1).Span(2).Id != "\r\n")
                {
                    var t = compiler.DeclarationOptions(offset);
                    Tracer.Assert(t != null, () => (new Source(text) + i).Dump());
                }
            }
        }
    }
}