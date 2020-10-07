using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.UnitTest;
using Reni.FeatureTest.Helper;
using Reni.Validation;

namespace Reni.ParserTest
{
    [UnitTest]
    [Target(@"{!()}")]
    [Output("")]
    [LowPriority]
    public sealed class StrangeDeclarationSyntax : CompilerTest
    {
        public StrangeDeclarationSyntax()
        {
            // ReSharper disable once ArrangeConstructorOrDestructorBody
            Parameters.TraceOptions.Parser = false;
            //Parameters.TraceOptions.Parser = true;
            //Parameters.ParseOnly = true;
        }

        protected override void Verify(IEnumerable<Issue> issues)
        {
            var issueArray = issues.ToArray();
            var i = 0;
            Tracer.Assert(issueArray[i].IssueId == IssueId.InvalidExpression, issueArray[i].Dump);
            i++;
            Tracer.Assert(i == issueArray.Length);
        }
    }
}