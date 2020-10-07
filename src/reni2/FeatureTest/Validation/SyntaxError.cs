using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using hw.UnitTest;
using NUnit.Framework;
using Reni.FeatureTest.Helper;
using Reni.Parser;
using Reni.Validation;

namespace Reni.FeatureTest.Validation
{
    [UnitTest]
    [TestFixture]
    [Target(value: @"1 #(x asdf y)# dump_print")]
    [Output(value: "")]
    [UseOfUndefinedContextSymbol]
    public sealed class SyntaxErrorComment : CompilerTest
    {
        public SyntaxErrorComment() => Parameters.ParseOnly = true;

        [Test]
        public new void Run() => base.Run();

        protected override void Verify(IEnumerable<Issue> issues)
        {
            var a = issues.ToArray();
            var i = 0;
            Tracer.Assert
                (a[i++].IsLogDumpLike(1, 3, 1, 26, IssueId.EOFInComment, "\"#(x asdf y)# dump_print\""));
            Tracer.Assert(a.Length == i);
        }
    }

    [UnitTest]
    [Target
        (
            value: @"
' hallo
world'
")]
    [Output(value: "")]
    public sealed class SyntaxErrorString : CompilerTest
    {
        public SyntaxErrorString() => Parameters.ParseOnly = true;

        protected override void Verify(IEnumerable<Issue> issues)
        {
            var a = issues.ToArray();
            var i = 0;
            Tracer.Assert(a[i++].IsLogDumpLike(2, 1, 3, 1, IssueId.EOLInString, "\"' hallo\n\""));
            Tracer.Assert(a[i++].IsLogDumpLike(3, 6, 4, 1, IssueId.EOLInString, "\"'\n\""));
            Tracer.Assert(a.Length == i);
        }
    }

    static class SyntaxErrorExtension
    {
        const string Pattern = ".reni({0},{1},{2},{3}): error {4}: ";

        const string RegExPattern = ".*\\.reni\\({0},{1}\\): error {2}: (.*)";

        internal static bool IsLogDumpLike
        (
            this Issue target,
            int line,
            int column,
            int lineEnd,
            int columnEnd,
            IssueId issueId,
            string expectedText)
        {
            if(target.IssueId != issueId)
                return false;

            var logDump = target.LogDump;

            var pattern = Pattern.ReplaceArgs(line, column, lineEnd, columnEnd, issueId);
            var match = pattern.Box().Find;

            var start = match.Apply(logDump);
            if(start == null)
                return false;

            var logText = logDump.Substring(start.Value);
            if(logText.StartsWith(expectedText))
                return true;

            return false;
        }
    }
}