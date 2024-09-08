using hw.UnitTest;
using Reni.FeatureTest.Helper;
using Reni.Validation;

namespace ReniUI.Generated.At240831_194341;

[UnitTest]
public sealed class Test : CompilerTest
{
    protected override string Target => (SmbFile.SourceFolder / "Test.reni").String;

    protected override void Verify(IEnumerable<Issue> issues)
    {
        var issueArray = issues.ToArray();
        var i = 0;
        var issueBase = issueArray[i];
        (issueBase.IssueId == IssueId.ExtraLeftBracket).Assert(issueBase.Dump);
        i++;
        issueBase = issueArray[i];
        (issueBase.IssueId == IssueId.MissingMatchingRightBracket).Assert(issueBase.Dump);
        i++;
        issueBase = issueArray[i];
        (issueBase.IssueId == IssueId.InvalidExpression).Assert(issueBase.Dump);
        i++;
        (i == issueArray.Length).Assert();
    }
}
