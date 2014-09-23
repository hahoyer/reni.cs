using System.Linq;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;
using hw.Debug;
using hw.Helper;
using hw.UnitTest;
using Reni.Validation;

namespace Reni.FeatureTest.Validation
{
    [TestFixture]
    [Target(@"1 #(x asdf y)# dump_print")]
    [Output("")]
    [UseOfUndefinedContextSymbol]
    public sealed class SyntaxErrorComment : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
        protected override void Verify(IEnumerable<IssueBase> issues)
        {
            var issueArray = issues.ToArray();
            var i = 0;
            Tracer.Assert(issueArray[i++].IsLogdumpLike(1, 2, IssueId.EOFInComment, " #(x asdf y)# dump_print"));
            Tracer.Assert(issueArray.Length == i);
        }
    }

    [TestFixture]
    [Target(@"
' hallo
world'
")]
    [Output("")]
    public sealed class SyntaxErrorString : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
        protected override void Verify(IEnumerable<IssueBase> issues)
        {
            var issueArray = issues.ToArray();
            var i = 0;
            Tracer.Assert(issueArray[i++].IsLogdumpLike(2, 1, IssueId.EOLInString, "' hallo\r"));
            Tracer.Assert(issueArray.Length == i);
        }
    }

    static class SyntaxErrorExpansion
    {
        const string Pattern = ".*\\.reni\\({0},{1}\\): error {2}: (.*)";

        internal static bool IsLogdumpLike(this IssueBase target, int line, int column, IssueId issueId, string text)
        {
            if(target.IssueId != issueId)
                return false;

            return new Regex(Pattern.ReplaceArgs(line, column, issueId)).Match(target.LogDump).Groups[1]
                .Value
                == text;
        }
    }
}