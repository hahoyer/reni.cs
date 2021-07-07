using hw.DebugFormatter;
using hw.UnitTest;
using NUnit.Framework;

namespace ReniUI.Test
{
    [UnitTest]
    [TestFixture]
    [PairedSyntaxTree]
    [Formatting]
    public sealed class FormattingOfBadThings : DependenceProvider
    {
        [UnitTest]
        [Test]
        public void BadArgDeclaration()
        {
            const string text = @"{^   :   ^}";
            var compiler = CompilerBrowser.FromText(text);
            var span = (compiler.Source + 0).Span(text.Length);
            var x = compiler.Reformat(targetPart: span);

            (x == "{^ : ^}").Assert(x);
        }
    }
}