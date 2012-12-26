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

using System.Linq;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.UnitTest;
using Reni.Validation;

namespace Reni.FeatureTest.Validation
{
    [TestFixture]
    [Target(@"1 #(x asdf y)# dump_print")]
    [Output("")]
    [UseOfUndefinedContextSymbol]
    public sealed class SyntaxErrorComment : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
        protected override void Verify(IEnumerable<IssueBase> issues)
        {
            var issueArray = issues.ToArray();
            var i = 0;
            Tracer.Assert(issueArray[i++].IsLogdumpLike(1, 2, IssueId.EOFInComment, " #(x asdf y)# dump_print"));
            Tracer.Assert(issueArray.Length == i);
        }
    }

    [TestFixture]
    [Target(@"
' hallo
world'
")]
    [Output("")]
    public sealed class SyntaxErrorString : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
        protected override void Verify(IEnumerable<IssueBase> issues)
        {
            var issueArray = issues.ToArray();
            var i = 0;
            Tracer.Assert(issueArray[i++].IsLogdumpLike(2, 1, IssueId.EOLInString, "' hallo\r"));
            Tracer.Assert(issueArray[i++].IsLogdumpLike(3, 6, IssueId.EOLInString, "'\r"));
            Tracer.Assert(issueArray.Length == i);
        }
    }

    static class SyntaxErrorExpansion
    {
        const string Pattern = ".*\\.reni\\({0},{1}\\): error {2}: (.*)";

        internal static bool IsLogdumpLike(this IssueBase target, int line, int column, IssueId issueId, string text)
        {
            if(target.IssueId != issueId)
                return false;

            return new Regex(Pattern.ReplaceArgs(line, column, issueId)).Match(target.LogDump).Groups[1]
                       .Value
                   == text;
        }
    }
}