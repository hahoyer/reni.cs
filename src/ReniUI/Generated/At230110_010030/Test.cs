using hw.DebugFormatter;
using hw.Helper;
using Reni.FeatureTest.Helper;
using Reni.Validation;

namespace ReniUI.Generated.At230110_010030;

//[UnitTest]
public class Test : CompilerTest
{
    protected override string Target => (SmbFile.SourceFolder / "Text.reni").String;

    protected override void Verify(IEnumerable<Issue> issues)
    {
        var issueArray = issues.ToArray();
        var i = 0;
        var issueBase = issueArray[i];
        (issueBase.IssueId == IssueId.MissingDeclarationValue).Assert(issueBase.Dump);
        i++;
        issueBase = issueArray[i];
        (issueBase.IssueId == IssueId.InvalidDeclaration).Assert(issueBase.Dump);
        i++;
        (i == issueArray.Length).Assert();
    }
}