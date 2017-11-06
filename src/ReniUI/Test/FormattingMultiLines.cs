using System.Linq;
using hw.DebugFormatter;
using hw.UnitTest;
using NUnit.Framework;
using ReniUI.Formatting;

namespace ReniUI.Test {
    [UnitTest]
    [TestFixture]
    public sealed class FormattingMultiLines : DependantAttribute
    {
        [Test]
        [UnitTest]
        public void ReformatComments()
        {
            const string Text =
                @"(12345,12345,12345,12345,12345,12345,12345,12345,12345)";

            const string ExpectedText =
                @"137;

3";
            var compiler = CompilerBrowser.FromText(Text);
            var newSource = compiler.Reformat
            (
                new ReniUI.Formatting.Configuration
                    {
                        EmptyLineLimit = 2,
                        MaxLineLength = 20
                    }
                    .Create()
            );

            var lineCount = newSource.Count(item => item == '\n');

            Tracer.Assert
            (
                newSource.Replace("\r\n", "\n") == ExpectedText.Replace("\r\n", "\n"),
                "\n\"" + newSource + "\"");
        }
    }
}