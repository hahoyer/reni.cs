using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.UnitTest;

namespace Reni.FeatureTest.UserInterface
{
    [UnitTest]
    public sealed class ExpressionFormatting : DependantAttribute
    {
        [UnitTest]
        public void FromSourcePart()
        {
            const string Text = @"(1,3,4,6)";
            var compiler = new Compiler(text: Text);
            var span = (compiler.Source + 0).Span(Text.Length);
            var x = compiler.Containing(span).Reformat(span);
            Tracer.Assert(x == "(1, 3, 4, 6)", x);
        }

        [UnitTest]
        public void CommentFromSourcePart()
        {
            const string Text = @"( # Comment
1,3,4,6)";
            var compiler = new Compiler(text: Text);
            var span = (compiler.Source + 2).Span(3);
            var x = compiler.Containing(span).Reformat(span);

            Tracer.Assert(x == "# C", x);
        }
    }
}