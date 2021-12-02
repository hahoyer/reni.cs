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
            Parameters.CompilationLevel = CompilationLevel.Syntax;
        }

        protected override void Verify(IEnumerable<Issue> issues)
        {
            var issueArray = issues.ToArray();
            var i = 0;
            var issueBase = issueArray[i];
            (issueBase.IssueId == IssueId.MissingRightBracket).Assert(issueBase.Dump);
            (issueBase.Position.GetDumpAroundCurrent(2) == "... ([] }...").Assert(issueBase.Dump);
            i++;
            (i == issueArray.Length).Assert(()=>Tracer.Dump(issueArray));
        }
    }

    [UnitTest]
    [Target(@"x:{ 1 x ) }; 1 dump_print")]
    public sealed class UnMatchedRightParenthesis : CompilerTest
    {
        public UnMatchedRightParenthesis() 
            => Parameters.CompilationLevel = CompilationLevel.Syntax;

        protected override void Verify(IEnumerable<Issue> issues)
        {
            var issueArray = issues.ToArray();
            var i = 0;
            var issueBase = issueArray[i];
            (issueBase.IssueId == IssueId.MissingLeftBracket).Assert(issueBase.Dump);
            i++;
            (i == issueArray.Length).Assert();
        }
    }


    [UnitTest]
    [UnMatchedLeftParenthesis]
    [UnMatchedRightParenthesis]
    public sealed class UnMatchedBrackets : CompilerTest {}
}