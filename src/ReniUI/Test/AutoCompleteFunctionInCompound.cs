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
    public sealed class AutoCompleteFunctionInCompound : DependenceProvider
    {
        const string text = @"
    NewMemory: @
    {
        result: ^
    }
    result
";

        [UnitTest]
        public void GetDeclarationOptions()
        {
            var compiler = CompilerBrowser.FromText(text);
            var offset = compiler.Source.Data.IndexOf("}", StringComparison.InvariantCulture) + 1;
            var position = compiler.Source + offset;
            Tracer.Assert(position.Span(1).Id == "\n");
            
            var t = compiler.DeclarationOptions(offset);
            Tracer.Assert(t != null, () => (new Source(text) + offset).Dump());
        }
    }
}