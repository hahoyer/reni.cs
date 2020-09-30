using System.Linq;
using hw.DebugFormatter;
using hw.UnitTest;
using NUnit.Framework;
using ReniUI.Formatting;

namespace ReniUI.Test
{
    [UnitTest]
    [StructFormatting]
    public sealed class StructFormattingCurrent : DependenceProvider
    {

        [Test]
        [UnitTest]
        public void Start() {@"aaaaa 

bbbbb".SimpleTest(expected: @"aaaaa
bbbbb",
            maxLineLength: 10, emptyLineLimit:1);}

        [Test]
        [UnitTest]
        public void LabeledEntriesInList()
        {
            const string Text =
                @"{aaaaa, label: bbbbb
    }";

            var expectedText =
                @"{
    aaaaa,

    label:
        bbbbb
}"
                    .Replace("\r\n", "\n");


            var compiler = CompilerBrowser.FromText(Text);
            var newSource = compiler.Reformat
                (
                    new ReniUI.Formatting.Configuration
                    {
                        MaxLineLength = 10,
                        EmptyLineLimit = 0
                    }.Create()
                )
                .Replace("\r\n", "\n");

            var lineCount = newSource.Count(item => item == '\n');

            Tracer.Assert(newSource == expectedText, "\n\"" + newSource + "\"");
        }
    }
}