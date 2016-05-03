using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
using hw.UnitTest;
using NUnit.Framework;
using Reni;

namespace ReniUI.Test
{
    [UnitTest]
    [TestFixture]
    public sealed class TokenLocating : DependantAttribute
    {
        [UnitTest]
        public void FromSourcePart()
        {
            const string text = @"(1,3,4,6)";
            var compiler = CompilerBrowser.FromText(text);
            var span = (compiler.Source + 2).Span(3);
            var x = compiler.Locate(span).SourcePart;

            Tracer.Assert(x.Id == "1,3,4,6", x.Dump);
        }

        [UnitTest]
        public void CommentFromSourcePart()
        {
            const string text = @"( # Comment
1,3,4,6)";
            var compiler = CompilerBrowser.FromText(text);
            var span = compiler.Source + 2;
            var x = compiler.LocatePosition(span);
            var sourcePart = x.SourcePart;
            Tracer.Assert(sourcePart.Id.Replace("\r", "") == "# Comment\n", x.Dump);
        }


        [UnitTest]
        [Test]
        public void CompoundSourcePart()
        {
            const string text = @"(1,3,4,6)";
            var compiler = CompilerBrowser.FromText(text);
            SourcePosn span = compiler.Source + text.IndexOf(',');
            var x = compiler.LocatePosition(span).Syntax.Value.Target.SourcePart;
            Tracer.Assert(x.Id.Replace("\r", "") == "1,3,4,6", x.Dump);
        }

        [UnitTest]
        [Test]
        public void NamedCompoundSourcePart()
        {
            const string text = @"(x:1,3,4,6)";
            var compiler = CompilerBrowser.FromText(text);
            SourcePosn span = compiler.Source + text.IndexOf(',');
            var x = compiler.LocatePosition(span).Syntax.Value.Target.SourcePart;
            Tracer.Assert(x.Id.Replace("\r", "") == "x:1,3,4,6", x.Dump);
        }
    }
}