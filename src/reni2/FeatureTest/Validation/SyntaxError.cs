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
        protected override void Verify(IEnumerable<IssueBase> issues)
        {
            var issueArray = issues.ToArray();
            var i = 0;
            Tracer.Assert
                (
                    issueArray[i++].IsLogdumpLike
                        (1, 2, IssueId.EOFInComment, " #(x asdf y)# dump_print"));
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
        protected override void Verify(IEnumerable<IssueBase> issues)
        {
            var issueArray = issues.ToArray();
            var i = 0;
            Tracer.Assert(issueArray[i++].IsLogdumpLike(3, 6, IssueId.EOLInString, "'"));
            Tracer.Assert(issueArray[i++].IsLogdumpLike(2, 1, IssueId.EOLInString, "' hallo"));
            Tracer.Assert(issueArray.Length == i);
        }
    }

    static class SyntaxErrorExtension
    {
        const string Pattern = ".*\\.reni\\({0},{1}\\): error {2}: (.*)";

        internal static bool IsLogdumpLike
            (this IssueBase target, int line, int column, IssueId issueId, string text)
        {
            if(target.IssueId != issueId)
                return false;

            var value =
                new Regex(Pattern.ReplaceArgs(line, column, issueId)).Match
                    (target.LogDump.Replace("\r", "")).Groups[1]
                    .Value;
            if(value != text)
                return false;
            return true;
        }
    }
}