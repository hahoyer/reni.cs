using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.UnitTest;
using Reni.FeatureTest.Helper;
using Reni.FeatureTest.Structure;
using Reni.FeatureTest.Validation;
using Reni.Validation;

namespace Reni.FeatureTest;

[UnitTest]
[Target(@"!public x: 1")]
[Output("")]
public sealed class ScopeHandlingPublic : CompilerTest { }

[UnitTest]
[Target(@"!non_public x: 1")]
[Output("")]
public sealed class ScopeHandlingNonPublic : CompilerTest { }

[UnitTest]
[Target(@"!(public mutable) x: 1")]
[ScopeHandlingPublic]
[Output("")]
public sealed class ScopeHandlingGroup : CompilerTest { }

[UnitTest]
[Target(@"!public !mutable x: 1")]
[ScopeHandlingPublic]
[Output("")]
public sealed class ScopeHandlingMultiple : CompilerTest { }

[UnitTest]
[Target(@"!unkown x: 1")]
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
[Target(@"a:(!non_public x: 1; !public y: 2); a x dump_print")]
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
        (issueBase.IssueId == IssueId.MissingDeclarationForType).Assert(issueBase.Dump);
        (issueBase.Position.Id == "dump_print").Assert(issueBase.Dump);
        i++;
        (i == issueArray.Length).Assert();
    }
}

[UnitTest]
[Target(@"a: (!non_public x: 1; !public y: 2); a y dump_print")]
[DumpPrint]
[UndefinedSymbol]
[ScopeHandlingPublic]
[ScopeHandlingNonPublic]
[Output("2")]
public sealed class PublicNonPublic2 : CompilerTest { }

[UnitTest]
[ScopeHandlingPublic]
[ScopeHandlingNonPublic]
[PublicNonPublic1]
[PublicNonPublic2]
[ScopeHandlingGroup]
[ScopeHandlingError]
[ScopeHandlingMultiple]
public sealed class AllScopeHandling : CompilerTest { }