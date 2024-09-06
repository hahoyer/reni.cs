using hw.UnitTest;

namespace ReniUI.Test.Formatting
{
    [UnitTest]
    [TestFixture]
    [PairedSyntaxTree]
    [Complex]
    public sealed class BadThings : DependenceProvider
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