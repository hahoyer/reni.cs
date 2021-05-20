using System.Diagnostics;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.UnitTest;
using NUnit.Framework;
using ReniUI.Formatting;

namespace ReniUI.Test
{
    [UnitTest]
    [TestFixture]
    [StructFormattingCurrent]
    public sealed class FormattingSimple : DependenceProvider
    {

        [Test]
        [UnitTest]
        public void ReformatComments()
        {
            const string Text =
                @"137;

################################################################
# Test
################################################################
                   3
";

            const string ExpectedText =
                @"137;

################################################################
# Test
################################################################
3
";
            var compiler = CompilerBrowser.FromText(Text);
            var newSource = compiler.Reformat
            (
                new ReniUI.Formatting.Configuration
                        {EmptyLineLimit = 2}
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