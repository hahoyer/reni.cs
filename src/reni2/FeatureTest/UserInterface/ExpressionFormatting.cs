using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.UnitTest;
using NUnit.Framework;

namespace Reni.FeatureTest.UserInterface
{
    [UnitTest]
    [TestFixture]
    public sealed class ExpressionFormatting : DependantAttribute
    {
        [UnitTest]
        public void FromSourcePart()
        {
            const string Text = @"(1,3,4,6)";
            var compiler = Compiler.BrowserFromText(text: Text);
            var span = (compiler.Source + 0).Span(Text.Length);
            var x = compiler.Locate(span).Reformat(span);
            Tracer.Assert(x == "(1, 3, 4, 6)", x);
        }

        [UnitTest]
        public void CommentFromSourcePart()
        {
            const string Text = @"( # Comment
1,3,4,6)";
            var compiler = Compiler.BrowserFromText(text: Text);
            var span = (compiler.Source + 2).Span(3);
            var locate = compiler.Locate(span);
            var x = locate.Reformat(span);

            Tracer.Assert(x == "# C", x);
        }

        [UnitTest]
        [Test]
        public void BadArgDeclaration()
        {
            const string Text = @"{^ : ^}";
            var compiler = Compiler.BrowserFromText(text: Text);
            var span = (compiler.Source + 0).Span(Text.Length);
            var x = compiler.Locate(span).Reformat(span);

            Tracer.Assert(x == "{^ : ^}", x);
        }
    }
}