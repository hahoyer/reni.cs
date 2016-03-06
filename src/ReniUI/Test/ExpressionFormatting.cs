using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.UnitTest;
using NUnit.Framework;
using Reni;

namespace ReniUI.Test
{
    [UnitTest]
    [TestFixture]
    public sealed class ExpressionFormatting : DependantAttribute
    {
        [UnitTest]
        public void FromSourcePart()
        {
            const string Text = @"(1,3,4,6)";
            var compiler = CompilerBrowser.FromText(Text);
            var span = (compiler.Source + 0).Span(Text.Length);
            var x = compiler.Locate(span).Reformat(span);
            Tracer.Assert(x == "(1, 3, 4, 6)", x);
        }

        [UnitTest]
        [Test]
        public void BadArgDeclaration()
        {
            const string Text = @"{^ : ^}";
            var compiler = CompilerBrowser.FromText(Text);
            var span = (compiler.Source + 0).Span(Text.Length);
            var x = compiler.Locate(span).Reformat(span);

            Tracer.Assert(x == "{^ : ^}", x);
        }
    }
}