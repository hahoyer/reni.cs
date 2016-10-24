using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.UnitTest;
using Reni.FeatureTest.Helper;
using Reni.Validation;

namespace Reni.FeatureTest
{
    [UnitTest]
    [Target(@"!public x: 1")]
    [Output("")]
    public sealed class ScopeHandlingPublic : CompilerTest {}

    [UnitTest]
    [Target(@"!non_public x: 1")]
    [Output("")]
    public sealed class ScopeHandlingNonPublic : CompilerTest {}

    [UnitTest]
    [Target(@"!(public mutable) x: 1")]
    [ScopeHandlingPublic]
    [Output("")]
    public sealed class ScopeHandlingGroup : CompilerTest {}

    [UnitTest]
    [Target(@"!public !mutable x: 1")]
    [ScopeHandlingPublic]
    [Output("")]
    public sealed class ScopeHandlingMultiple : CompilerTest {}

    [UnitTest]
    [Target(@"!unkown x: 1")]
    [Output("")]
    public sealed class ScopeHandlingError : CompilerTest {}

    [UnitTest]
    [Target(@"a:(!non_public x: 1; !public y: 2); a x dump_print")]
    [ScopeHandlingPublic]
    [ScopeHandlingNonPublic]
    public sealed class PublicNonPublic1 : CompilerTest
    {
        public PublicNonPublic1() { Parameters.ProcessErrors = true; }

        protected override void Verify(IEnumerable<Issue> issues)
        {
            var issueArray = issues.ToArray();
            var i = 0;
            var issueBase = issueArray[i];
            Tracer.Assert(issueBase.IssueId == IssueId.MissingDeclarationInContext, issueBase.Dump);
            Tracer.Assert(issueBase.Position.Id == "x", issueBase.Dump);
            i++;
            issueBase = issueArray[i];
            Tracer.Assert(issueBase.IssueId == IssueId.ConsequentialError, issueBase.Dump);
            Tracer.Assert(issueBase.Position.Id == "dump_print", issueBase.Dump);
            i++;
            Tracer.Assert(i == issueArray.Length);
        }

    }

    [UnitTest]
    [Target(@"a: (!non_public x: 1; !public y: 2); a y dump_print")]
    [ScopeHandlingPublic]
    [ScopeHandlingNonPublic]
    [Output("2")]
    public sealed class PublicNonPublic2 : CompilerTest {}

    [UnitTest]
    [ScopeHandlingPublic]
    [ScopeHandlingNonPublic]
    [PublicNonPublic1]
    [PublicNonPublic2]
    [ScopeHandlingGroup]
    [ScopeHandlingError]
    [ScopeHandlingMultiple]
    public sealed class AllScopeHandling : CompilerTest {}
}