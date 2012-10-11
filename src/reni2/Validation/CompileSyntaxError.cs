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
using HWClassLibrary.Helper;
using Reni.Basics;
using Reni.Context;
using Reni.Parser;
using Reni.Syntax;

namespace Reni.Validation
{
    sealed class CompileSyntaxError : CompileSyntax
    {
        [EnableDump]
        readonly IssueId _issueId;
        readonly CompileSyntaxError _next;
        readonly SimpleCache<CompileSyntaxIssue> _issueCache;

        public CompileSyntaxError(TokenData token, IssueId issueId, CompileSyntaxError next = null)
            : base(token)
        {
            _issueId = issueId;
            _next = next;
            _issueCache = new SimpleCache<CompileSyntaxIssue>(() => new CompileSyntaxIssue(_issueId, Token));
        }

        CompileSyntaxIssue Issue { get { return _issueCache.Value; } }

        internal override Result ObtainResult(ContextBase context, Category category)
        {
            var result = Chain
                .Select(error => error.IssueType(context).IssueResult(category))
                .Aggregate(context.RootContext.VoidType.Result(category), (x, y) => x + y);
            return result;
        }

        [DisableDump]
        IEnumerable<CompileSyntaxError> Chain
        {
            get
            {
                var current = this;
                do
                {
                    yield return current;
                    current = current._next;
                } while(current != null);
            }
        }

        IssueType IssueType(ContextBase context) { return new IssueType(Issue, context.RootContext); }
    }
}