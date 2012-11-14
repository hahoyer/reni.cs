#region Copyright (C) 2012

//     Project Reni2
//     Copyright (C) 2012 - 2012 Harald Hoyer
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//     
//     Comments, bugs and suggestions to hahoyer at yahoo.de

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.UnitTest;
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