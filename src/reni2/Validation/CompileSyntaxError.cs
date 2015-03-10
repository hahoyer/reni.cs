using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using hw.Parser;
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

        public CompileSyntaxError
            (
            CompileSyntaxError other,
            params ParsedSyntax[] parts)
            : base(other, parts)
        {
            _issueId = other._issueId;
            _previous = other._previous;
            _issueCache = new ValueCache<CompileSyntaxIssue>
                (() => new CompileSyntaxIssue(_issueId, Token));
        }

        internal override IEnumerable<IssueBase> Issues => Issue.plus(base.Issues);

        public CompileSyntaxError
            (
            IssueId issueId,
            Token token,
            CompileSyntaxError previous = null)
            : base(token)
        {
            _issueId = issueId;
            _previous = previous;
            _issueCache = new ValueCache<CompileSyntaxIssue>
                (() => new CompileSyntaxIssue(_issueId, Token));
        }

        CompileSyntaxIssue Issue => _issueCache.Value;

        internal override Result ResultForCache(ContextBase context, Category category)
        {
            var result = Chain
                .Select(error => error.IssueType(context).IssueResult(category))
                .Aggregate(context.RootContext.VoidType.Result(category), (x, y) => x + y);
            return result;
        }
        internal override CompileSyntax SurroundCompileSyntax(params ParsedSyntax[] parts)
            => new CompileSyntaxError(this, parts);

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

        IssueType IssueType(ContextBase context) => new IssueType(Issue, context.RootContext);

        internal override bool IsError => true;
        internal override Syntax SyntaxError
            (IssueId issue, Token token, Syntax right = null, params ParsedSyntax[] parts)
            => new CompileSyntaxError(issue, token, this).SurroundCompileSyntax(parts);

        [DisableDump]
        protected override IEnumerable<Syntax> SyntaxChildren { get { yield return _previous; } }
    }
}