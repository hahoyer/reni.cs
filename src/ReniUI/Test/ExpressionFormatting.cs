using hw.DebugFormatter;
using hw.UnitTest;
using NUnit.Framework;

namespace ReniUI.Test
{
    [UnitTest]
    [TestFixture]
    public sealed class ExpressionFormatting : DependenceProvider
    {
        [UnitTest]
        public void FromSourcePart()
        {
            const string text = @"(1,3,4,6)";
            var compiler = CompilerBrowser.FromText(text);
            var span = (compiler.Source + 0).Span(text.Length);
            var x = compiler.Reformat(targetPart: span);
            Tracer.Assert(x == "(1, 3, 4, 6)", x);
        }

        [UnitTest]
        [Test]
        public void BadArgDeclaration()
        {
            const string text = @"{^ : ^}";
            var compiler = CompilerBrowser.FromText(text);
            var span = (compiler.Source + 0).Span(text.Length);
            var x = compiler.Reformat(targetPart: span);

            Tracer.Assert(x == "{^ : ^}", x);
        }
    }
}