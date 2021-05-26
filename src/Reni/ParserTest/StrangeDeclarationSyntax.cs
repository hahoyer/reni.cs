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
    public sealed class StrangeDeclarationSyntax : CompilerTest
    {
        public StrangeDeclarationSyntax()
        {
            // ReSharper disable once ArrangeConstructorOrDestructorBody
            Parameters.TraceOptions.Parser = false;
            Parameters.CompilationLevel = CompilationLevel.Syntax;
            //Parameters.TraceOptions.Parser = true;
            //Parameters.ParseOnly = true;
        }

        protected override void Verify(IEnumerable<Issue> issues)
        {
            var issueArray = issues.ToArray();
            var i = 0;
            (issueArray[i].IssueId == IssueId.InvalidExpression).Assert(issueArray[i].Dump);
            i++;
            (i == issueArray.Length).Assert();
        }
    }
}