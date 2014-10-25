using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.ReniSyntax;

namespace Reni.Validation
{
    sealed class CompileSyntaxError : CompileSyntax
    {
        [EnableDump]
        readonly IssueId _issueId;
        readonly CompileSyntaxError _next;
        readonly ValueCache<CompileSyntaxIssue> _issueCache;

        public CompileSyntaxError(SourcePart token, IssueId issueId, CompileSyntaxError next = null)
            : base(token)
        {
            _issueId = issueId;
            _next = next;
            _issueCache = new ValueCache<CompileSyntaxIssue>(() => new CompileSyntaxIssue(_issueId, Token));
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