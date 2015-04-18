using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.UnitTest;
using Reni.Formatting;
using Reni.TokenClasses;

namespace Reni.FeatureTest.UserInterface
{
    [UnitTest]
    public sealed class ExpressionFormatting : DependantAttribute
    {
        [UnitTest]
        public void FromSourcePartSimple()
        {
            const string Text = @"1";
            var compiler = new Compiler(text: Text);
            var x = compiler.SourceSyntax.
                Reformat();
            Tracer.Assert(x == "1", x);
        }

        [UnitTest]
        public void FromSourcePart()
        {
            const string Text = @"(1,3,4,6)";
            var compiler = new Compiler(text: Text);
            var x = compiler.SourceSyntax.
                Reformat();
            Tracer.Assert(x == "(1, 3, 4, 6)", x);
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