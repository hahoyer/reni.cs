using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Scanner;
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
            const string Text = @"(1,3,4,6)";
            var compiler = new Compiler(text: Text);
            var span = (compiler.Source + 2).Span(3);
            var x = ((SyntaxToken)compiler.Token(span)).SourceSyntax.SourcePart;

            Tracer.Assert(x.Id == "1,3,4,6", x.Dump);

        }

        [UnitTest]
        public void CommentFromSourcePart()
        {
            const string Text = @"( # Comment
1,3,4,6)";
            var compiler = new Compiler(text: Text);
            var span = (compiler.Source + 2).Span(3);
            var x = compiler.Token(span);

            Tracer.Assert(x.SourcePart.Id == "# Comment\r", x.Dump);

        }

    }
}