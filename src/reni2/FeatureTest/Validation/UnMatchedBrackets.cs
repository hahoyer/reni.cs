using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.UnitTest;
using Reni.FeatureTest.Helper;
using Reni.Validation;

namespace Reni.FeatureTest.Validation
{
    [UnitTest]
    [Target(@"x:{ 1 x ( }; 1 dump_print")]
    public sealed class UnMatchedLeftParenthesis : CompilerTest
    {
        public UnMatchedLeftParenthesis()
        {
            Parameters.TraceOptions.Parser = false;
            Parameters.ParseOnly = true;
        }

        protected override void Verify(IEnumerable<Issue> issues)
        {
            var issueArray = issues.ToArray();
            var i = 0;
            var issueBase = issueArray[i];
            Tracer.Assert(issueBase.IssueId == IssueId.ExtraLeftBracket, issueBase.Dump);
            i++;
            Tracer.Assert(i == issueArray.Length, ()=>Tracer.Dump(issueArray));
        }
    }

    [UnitTest]
    [Target(@"x:{ 1 x ) }; 1 dump_print")]
    public sealed class UnMatchedRightParenthesis : CompilerTest
    {
        public UnMatchedRightParenthesis() { Parameters.ParseOnly = true; }

        protected override void Verify(IEnumerable<Issue> issues)
        {
            var issueArray = issues.ToArray();
            var i = 0;
            var issueBase = issueArray[i];
            Tracer.Assert(issueBase.IssueId == IssueId.ExtraRightBracket, issueBase.Dump);
            i++;
            Tracer.Assert(i == issueArray.Length);
        }
    }


    [UnitTest]
    [UnMatchedLeftParenthesis]
    [UnMatchedRightParenthesis]
    public sealed class UnMatchedBrackets : CompilerTest {}
}