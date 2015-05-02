using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.UnitTest;
using NUnit.Framework;
using Reni.UserInterface;

namespace Reni.FeatureTest.UserInterface
{
    [UnitTest]
    [TestFixture]
    public sealed class Formatting : DependantAttribute
    {
        [Test]
        [UnitTest]
        public void LineCommentFromSourcePart()
        {
            const string Text = @"( # Comment
1,3,4,6)";
            var compiler = new Compiler(text: Text);
            var span = (compiler.Source + 2).Span(3);
            var trimmed = compiler.Containing(span).Reformat(span);

            Tracer.Assert(trimmed == "# C", trimmed);
        }

        [Test]
        [UnitTest]
        public void CommentFromSourcePart()
        {
            const string Text = @"( #(aa Comment aa)#
1,3,4,6)";
            var compiler = new Compiler(text: Text);
            var span = (compiler.Source + 2).Span(3);
            var reformat = compiler.Containing(span).Reformat(span);
            Tracer.Assert(reformat == "#(a", reformat);
        }
    }
}