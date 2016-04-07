using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.UnitTest;
using Reni;

namespace ReniUI.Test
{
    [UnitTest]
    public sealed class TokenLocating : DependantAttribute
    {
        [UnitTest]
        public void FromSourcePart()
        {
            const string text = @"(1,3,4,6)";
            var compiler = CompilerBrowser.FromText(text);
            var span = (compiler.Source + 2).Span(3);
            var x = compiler.Locate(span).SourcePart;

            Tracer.Assert(x.Id == "1,3,4,6", x.Dump);
        }

        [UnitTest]
        public void CommentFromSourcePart()
        {
            const string text = @"( # Comment
1,3,4,6)";
            var compiler = CompilerBrowser.FromText(text);
            var span = compiler.Source + 2;
            var x = compiler.LocatePosition(span);
            var sourcePart = x.SourcePart;
            Tracer.Assert(sourcePart.Id.Replace("\r", "") == "# Comment\n", x.Dump);
        }
    }
}