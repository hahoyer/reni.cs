using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.UnitTest;
using Reni.Validation;

namespace Reni.FeatureTest.Validation
{
    [TestFixture]
    [Target(@"x:{ 1 type instance  ( }; 1 dump_print")]
    public sealed class UnMatcheLeftParenthesis : CompilerTest
    {
        public UnMatcheLeftParenthesis() { Parameters.ParseOnly = true; }

        protected override void Verify(IEnumerable<IssueBase> issues)
        {
            var issueArray = issues.ToArray();
            var i = 0;
            var issueBase = issueArray[i];
            Tracer.Assert(issueBase is CompileSyntaxIssue, issueBase.Dump);
            Tracer.Assert(issueBase.IssueId == IssueId.MissingRightBracket, issueBase.Dump);
            i++;
            Tracer.Assert(i == issueArray.Length);
        }
    }
}