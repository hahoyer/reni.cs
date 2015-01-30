using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.ReniParser;
using Reni.ReniSyntax;

namespace Reni.Validation
{
    sealed class CompileSyntaxError : CompileSyntax
    {
        [EnableDump]
        readonly IssueId _issueId;
        readonly CompileSyntaxError _previous;
        readonly ValueCache<CompileSyntaxIssue> _issueCache;

        public CompileSyntaxError(IssueId issueId, SourcePart token)
            : this(issueId, token, null)
        {}

        CompileSyntaxError(IssueId issueId, SourcePart token, CompileSyntaxError previous)
            : base(token)
        {
            _issueId = issueId;
            _previous = previous;
            _issueCache = new ValueCache<CompileSyntaxIssue>(() => new CompileSyntaxIssue(_issueId, Token));
        }

        CompileSyntaxIssue Issue { get { return _issueCache.Value; } }

        internal override Result ResultForCache(ContextBase context, Category category)
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
                    current = current._previous;
                } while(current != null);
            }
        }

        IssueType IssueType(ContextBase context) { return new IssueType(Issue, context.RootContext); }

        internal override Syntax SyntaxError(IssueId issue, SourcePart token)
        {
            return new CompileSyntaxError(issue, token, this);
        }
    }
}