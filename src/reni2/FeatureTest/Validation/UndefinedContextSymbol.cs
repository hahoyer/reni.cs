using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.UnitTest;
using Reni.FeatureTest.Helper;
using Reni.Validation;

namespace Reni.FeatureTest.Validation
{
    [UnitTest]
    [Target(@"x")]
    [Output("")]
    public sealed class UndefinedContextSymbol : CompilerTest
    {
        public UndefinedContextSymbol() { Parameters.ProcessErrors = true; }

        protected override void Verify(IEnumerable<Issue> issues)
        {
            var issueArray = issues.ToArray();
            var i = 0;
            Tracer.Assert(issueArray[i].IssueId == IssueId.MissingDeclarationInContext, issueArray[i].Dump);
            i++;
            Tracer.Assert(i == issueArray.Length);
        }
    }

    [UnitTest]
    [Target(@"x: 3; x x")]
    [Output("")]
    public sealed class UndefinedSymbol : CompilerTest
    {
        public UndefinedSymbol() { Parameters.ProcessErrors = true; }

        protected override void Verify(IEnumerable<Issue> issues)
        {
            var issueArray = issues.ToArray();
            var i = 0;
            Tracer.Assert(issueArray[i].IssueId == IssueId.MissingDeclarationForType, issueArray[i].Dump);
            i++;
            Tracer.Assert(i == issueArray.Length);
        }
    }

    [UnitTest]
    [Target(@"x dump_print")]
    [Output("")]
    [UndefinedContextSymbol]
    public sealed class UseOfUndefinedContextSymbol : CompilerTest
    {
        public UseOfUndefinedContextSymbol() { Parameters.ProcessErrors = true; }

        protected override void Verify(IEnumerable<Issue> issues)
        {
            var issueArray = issues.ToArray();
            var i = 0;
            Tracer.Assert(issueArray[i].IssueId == IssueId.MissingDeclarationInContext, issueArray[i].Dump);
            i++;
            Tracer.Assert(i == issueArray.Length);
        }
    }

    [UnitTest]
    [Target(@"x x dump_print")]
    [Output("")]
    [UseOfUndefinedContextSymbol]
    public sealed class IndirectUseOfUndefinedContextSymbol : CompilerTest
    {
        public IndirectUseOfUndefinedContextSymbol() { Parameters.ProcessErrors = true; }

        protected override void Verify(IEnumerable<Issue> issues)
        {
            var issueArray = issues.ToArray();
            var i = 0;
            Tracer.Assert(issueArray[i].IssueId == IssueId.MissingDeclarationInContext, issueArray[i].Dump);
            i++;
            Tracer.Assert(i == issueArray.Length);
        }
    }
}