using hw.UnitTest;
using Reni.FeatureTest.Helper;
using Reni.FeatureTest.Structure;
using Reni.FeatureTest.Validation;
using Reni.Validation;

namespace Reni.FeatureTest;

[UnitTest]
[Target(@"x!public : 1")]
[Output("")]
public sealed class ScopeHandlingPublic : CompilerTest;

[UnitTest]
[Target(@"x!non_public : 1")]
[Output("")]
public sealed class ScopeHandlingNonPublic : CompilerTest;

[UnitTest]
[Target(@"x!(public, mutable) : 1")]
[ScopeHandlingPublic]
[Output("")]
public sealed class ScopeHandlingGroup : CompilerTest;

[UnitTest]
[Target(@"x!(public mutable) : 1")]
[ScopeHandlingPublic]
[Output("")]
public sealed class ScopeHandlingBadGroup : CompilerTest
{
    protected override void Verify(IEnumerable<Issue> issues)
    {
        var issue = issues.Single();
        (issue.IssueId == IssueId.InvalidDeclaration).Assert(()
            => $"Issue of type {IssueId.InvalidDeclaration} expected. \n"
            + $"Found: {issues.Select(issue => issue.LogDump).Stringify("\n")}");
    }
}


[UnitTest]
[Target(@"x!(public, mutable, converter, mix_in) : 1")]
[ScopeHandlingPublic]
[Output("")]
public sealed class ScopeHandlingGroup4 : CompilerTest;

[UnitTest]
[Target(@"<< !public : 1")]
[Output("")]
[ScopeHandlingPublic]
public sealed class ConcatDeclaration : CompilerTest;

[UnitTest]
[Target(@"x!public !mutable: 1")]
[ScopeHandlingPublic]
[Output("")]
public sealed class ScopeHandlingMultiple : CompilerTest;

[UnitTest]
[Target(@"x!unkown: 1")]
[Output("")]
public sealed class ScopeHandlingError : CompilerTest
{
    protected override void Verify(IEnumerable<Issue> issues)
    {
        var issue = issues.Single();
        (issue.IssueId == IssueId.InvalidDeclarationTag).Assert(()
            => $"Issue of type {IssueId.InvalidDeclarationTag} expected. \n"
            + $"Found: {issues.Select(issue => issue.LogDump).Stringify("\n")}");
    }
}

[UnitTest]
[Target(@"a:(x!non_public : 1; y!public: 2); a x dump_print")]
[ScopeHandlingPublic]
[UndefinedSymbol]
[DumpPrint]
[ScopeHandlingNonPublic]
public sealed class PublicNonPublic1 : CompilerTest
{
    public PublicNonPublic1() => Parameters.ProcessErrors = true;

    protected override void Verify(IEnumerable<Issue> issues)
    {
        var issueArray = issues.ToArray();
        var i = 0;
        var issueBase = issueArray[i];
        (issueBase.IssueId == IssueId.MissingDeclarationForType).Assert(issueBase.Dump);
        (issueBase.Position.Id == "x").Assert(issueBase.Dump);
        i++;
        issueBase = issueArray[i];
        (issueBase.IssueId == IssueId.ConsequenceError).Assert(issueBase.Dump);
        (issueBase.Position.Id == "dump_print").Assert(issueBase.Dump);
        i++;
        (i == issueArray.Length).Assert();
    }
}

[UnitTest]
[Target(@"a: (x!non_public: 1; y!public: 2); a y dump_print")]
[DumpPrint]
[UndefinedSymbol]
[ScopeHandlingPublic]
[ScopeHandlingNonPublic]
[Output("2")]
public sealed class PublicNonPublic2 : CompilerTest;

[UnitTest]
[ScopeHandlingPublic]
[ScopeHandlingNonPublic]
[PublicNonPublic1]
[PublicNonPublic2]
[ScopeHandlingGroup]
[ScopeHandlingBadGroup]
[ScopeHandlingGroup4]
[ScopeHandlingError]
[ScopeHandlingMultiple]
public sealed class AllScopeHandling : CompilerTest;