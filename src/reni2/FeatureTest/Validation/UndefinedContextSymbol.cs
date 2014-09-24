using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.UnitTest;
using Reni.Context;
using Reni.Parser;
using Reni.Validation;

namespace Reni.FeatureTest.Validation
{
    [TestFixture]
    [Target(@"x")]
    [Output("")]
    [LowPriority]
    [PrioTableTest]
    public sealed class UndefinedContextSymbol : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
        protected override void Verify(IEnumerable<IssueBase> issues)
        {
            var issue = issues.Single();
            Tracer.DumpStaticMethodWithData(issue);
            Tracer.TraceBreak();
        }
    }

    [TestFixture]
    [PrioTableTest]
    [Target(@"x: 3; x x")]
    [LowPriority]
    [Output("")]
    public sealed class UndefinedSymbol : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
        protected override void Verify(IEnumerable<IssueBase> issues) { var issue = (UndefinedSymbolIssue) issues.Single(); }
    }

    [TestFixture]
    [Target(@"x dump_print")]
    [Output("")]
    [UndefinedContextSymbol]
    public sealed class UseOfUndefinedContextSymbol : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
        protected override void Verify(IEnumerable<IssueBase> issues)
        {
            var issueArray = issues.ToArray();
            Tracer.Assert(issueArray.Length == 2);
            Tracer.Assert(issueArray[0] is UndefinedSymbolIssue);
            Tracer.Assert(issueArray[1] is ConsequentialError);
        }
    }

    [TestFixture]
    [Target(@"x x dump_print")]
    [Output("")]
    [UseOfUndefinedContextSymbol]
    public sealed class IndirectUseOfUndefinedContextSymbol : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
        protected override void Verify(IEnumerable<IssueBase> issues)
        {
            var issueArray = issues.ToArray();
            Tracer.Assert(issueArray.Length == 3);
            Tracer.Assert(issueArray[0] is UndefinedSymbolIssue);
            Tracer.Assert(issueArray[1] is ConsequentialError);
            Tracer.Assert(issueArray[2] is ConsequentialError);
        }
    }
}