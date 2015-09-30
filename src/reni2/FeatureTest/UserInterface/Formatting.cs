using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using hw.Debug;
using hw.Helper;
using hw.UnitTest;
using NUnit.Framework;
using Reni.Formatting;

namespace Reni.FeatureTest.UserInterface
{
    [UnitTest]
    [TestFixture]
    [LowPriority]
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
            var trimmed = compiler.ContainingTokens(span).Reformat(span);

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
            var reformat = compiler.ContainingTokens(span).Reformat(span);
            Tracer.Assert(reformat == "#(a", reformat);
        }

        [Test]
        [UnitTest]
        public void Reformat()
        {
            const string Text =
                @"systemdata:{1 type instance() Memory:((0 type *('100' to_number_of_base 64)) mutable) instance(); !mutable FreePointer: Memory array_reference mutable; repeat: /\ ^ while() then
    (
        ^ body(),
        repeat(^)
    );}; 1 = 1 then 2 else 4; 3; (Text('H') << 'allo') dump_print ";

            var compiler = new Compiler(text: Text);
            var newSource = compiler.SourceSyntax.Reformat
                (
                    provider:
                        new Provider
                        {
                            MaxLineLength = 100,
                            EmptyLineLimit = 0
                        }
                );

            var lineCount = newSource.Count(item => item == '\n');
            Tracer.Assert(lineCount == 9, "\n" + newSource);
        }

        [Test]
        [UnitTest]
        public void ReformatWithComments()
        {
            const string Text = @"
    X1_fork: object
    (

            X1_FullDump:
            (
            function
            (
                string('(') * X1_L X1_dump(arg) * ',' * X1_R X1_dump(arg) * ')';
        )
        );

            X1_dump:=
           (
           function
           (
           arg = 0 then string('...') else X1_FullDump(integer(arg - 1))
           )
           )
    );
            ";

            var compiler = new Compiler(text: Text);
            var newSource = compiler.SourceSyntax.Reformat
                (
                    provider:
                        new Provider
                        {
                            EmptyLineLimit = 1
                        }
                );

            var lineCount = newSource.Count(item => item == '\n');
            Tracer.Assert(lineCount == 23, "\n" + newSource);
        }

        [UnitTest]
        [Test]
        public void LegacySystem()
        {
            var srcDir = new StackTrace(true)
                .GetFrame(0)
                .GetFileName()
                .FileHandle()
                .DirectoryName
                + @"\..\..\..";
            var fileName = srcDir + @"\renisource\test.reni";
            var file = fileName.FileHandle();
            var compiler = new Compiler(fileName);
            var source = compiler.Source.All;
            var newSource = compiler.SourceSyntax.Reformat
                (
                    provider:
                        new Provider
                        {
                            EmptyLineLimit = 1
                        }
                );
            var lineCount = newSource.Count(item => item == '\n');
            Tracer.Assert(lineCount == 71, nameof(lineCount) + "=" + lineCount + "\n" + newSource);
        }
    }
}