using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
using hw.UnitTest;

namespace ReniUI.Test
{
    [UnitTest]
    [AutoCompleteSimple]
    public sealed class AutoCompleteFunctionInCompound : DependantAttribute
    {
        const string text = @"
    NewMemory: /\
    {
        result: ^
    }
    result
";

        [UnitTest]
        public void GetDeclarationOptions()
        {
            var compiler = CompilerBrowser.FromText(text);
            var offset = 64;
            var position = compiler.Source + offset;
            if((position + -1).Span(2).Id != "\r\n")
            {
                var t = compiler.DeclarationOptions(offset);
                Tracer.Assert(t != null, () => (new Source(text) + offset).Dump());
            }
        }
    }
}