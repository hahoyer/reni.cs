using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.UnitTest;
using Reni.FeatureTest.Helper;
using Reni.Validation;

namespace Reni.FeatureTest.Sample
{
    [UnitTest]
    [Target(value: @"!")]
    public sealed class Sample200104 : CompilerTest
    {
        public Sample200104()
        {
            Parameters.ParseOnly = true;
        }

        protected override void Verify(IEnumerable<Issue> issues)
        {
            var issueArray = issues.ToArray();
            var i = 0;
            var issueBase = issueArray[i];
            Tracer.Assert(issueBase.IssueId == IssueId.InvalidListOperandSequence, issueBase.Dump);
            i++;
            Tracer.Assert(i == issueArray.Length);
        }
    }
}