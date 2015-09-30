using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.UnitTest;
using Reni.UserInterface;

namespace Reni.FeatureTest.UserInterface
{
    [UnitTest]
    public sealed class TokenLocating : DependantAttribute
    {
        [UnitTest]
        public void FromSourcePart()
        {
            const string text = @"(1,3,4,6)";
            var compiler = new Compiler(text: text);
            var span = (compiler.Source + 2).Span(3);
            var x = ((SyntaxToken) compiler.ContainingTokens(span)).SourceSyntax.SourcePart;

            Tracer.Assert(x.Id == "1,3,4,6", x.Dump);
        }

        [UnitTest]
        public void CommentFromSourcePart()
        {
            const string text = @"( # Comment
1,3,4,6)";
            var compiler = new Compiler(text: text);
            var span = (compiler.Source + 2).Span(3);
            var x = compiler.ContainingTokens(span);

            Tracer.Assert(x.SourcePart.Id.Replace("\r", "") == "# Comment\n", x.Dump);
        }
    }
}