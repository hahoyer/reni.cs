using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.UnitTest;
using Reni.FeatureTest.Helper;
using Reni.Parser;
using Reni.Validation;

namespace Reni.FeatureTest.Validation
{
    [TestFixture]
    [Target(@"x")]
    [Output("")]
    [PrioTableTest]
    public sealed class UndefinedContextSymbol : CompilerTest
    {
        public UndefinedContextSymbol() { Parameters.ProcessErrors = true; }
        protected override void Verify(IEnumerable<SourceIssue> issues)
        {
            var issueArray = issues.ToArray();
            var i = 0;
            Tracer.Assert(issueArray[i] .IssueId == IssueId.UndefinedSymbol, issueArray[i].Dump);
            i++;
            Tracer.Assert(i == issueArray.Length);
        }
    }

    [TestFixture]
    [PrioTableTest]
    [Target(@"x: 3; x x")]
    [Output("")]
    public sealed class UndefinedSymbol : CompilerTest
    {
        public UndefinedSymbol() { Parameters.ProcessErrors = true; }
        protected override void Verify(IEnumerable<SourceIssue> issues)
        {
            var issueArray = issues.ToArray();
            var i = 0;
            Tracer.Assert(issueArray[i].IssueId == IssueId.UndefinedSymbol, issueArray[i].Dump);
            i++;
            Tracer.Assert(i == issueArray.Length);
        }
    }

    [TestFixture]
    [Target(@"x dump_print")]
    [Output("")]
    [LowPriority]
    [UndefinedContextSymbol]
    public sealed class UseOfUndefinedContextSymbol : CompilerTest
    {
        public UseOfUndefinedContextSymbol() { Parameters.ProcessErrors = true; }
        protected override void Verify(IEnumerable<SourceIssue> issues)
        {
            var issueArray = issues.ToArray();
            var i = 0;
            Tracer.Assert(issueArray[i].IssueId == IssueId.UndefinedSymbol, issueArray[i].Dump);
            i++;
            Tracer.Assert(issueArray[i].IssueId == IssueId.ConsequentialError, issueArray[i].Dump);
            i++;
            Tracer.Assert(i == issueArray.Length);
        }
    }

    [TestFixture]
    [Target(@"x x dump_print")]
    [Output("")]
    [UseOfUndefinedContextSymbol]
    public sealed class IndirectUseOfUndefinedContextSymbol : CompilerTest
    {
        public IndirectUseOfUndefinedContextSymbol() { Parameters.ProcessErrors = true; }
        protected override void Verify(IEnumerable<SourceIssue> issues)
        {
            var issueArray = issues.ToArray();
            var i = 0;
            Tracer.Assert(issueArray[i].IssueId == IssueId.UndefinedSymbol, issueArray[i].Dump);
            i++;
            Tracer.Assert(issueArray[i].IssueId == IssueId.ConsequentialError, issueArray[i].Dump);
            i++;
            Tracer.Assert(issueArray[i].IssueId == IssueId.ConsequentialError, issueArray[i].Dump);
            i++;
            Tracer.Assert(i == issueArray.Length);
        }
    }
}