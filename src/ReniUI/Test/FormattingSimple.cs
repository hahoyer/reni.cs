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
    public sealed class FormattingSimple : DependantAttribute
    {
        [Test]
        [UnitTest]
        public void CommentFromSourcePart()
        {
            const string Text = @"( #(aa Comment aa)#
1,3,4,6)";
            var compiler = CompilerBrowser.FromText(Text);
            var span = (compiler.Source + 2).Span(3);
            var reformat = compiler.Reformat(targetPart: span);
            Tracer.Assert(reformat == "#(a", reformat);
        }

        [UnitTest]
        [Formatting]
        [Test]
        public void LegacySystem()
        {
            var sourceDirectory
                = new StackTrace(true)
                      .GetFrame(0)
                      .GetFileName()
                      .ToSmbFile()
                      .DirectoryName +
                  @"\..\..";
            var fileName = sourceDirectory + @"\renisource\test.reni";
            var file = fileName.ToSmbFile();
            Tracer.Line(Tracer.FilePosn(fileName, 0, 0, 0, 0, "see there"));
            var compiler = CompilerBrowser.FromFile(fileName);
            var source = compiler.Source.All;
            var newSource = compiler.Reformat
            (
                new ReniUI.Formatting.Configuration
                    {EmptyLineLimit = 0}.Create
                    ()
            );
            var lineCount = newSource.Count(item => item == '\n');
            Tracer.Assert
                (lineCount == 57, nameof(lineCount) + "=" + lineCount + "\n" + newSource);
        }

        [Test]
        [UnitTest]
        public void LineCommentFromSourcePart()
        {
            const string Text = @"( # Comment
1,3,4,6)";
            var compiler = CompilerBrowser.FromText(Text);
            var span = (compiler.Source + 2).Span(3);
            var trimmed = compiler.Reformat(targetPart: span);

            Tracer.Assert(trimmed == "# C", trimmed);
        }

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