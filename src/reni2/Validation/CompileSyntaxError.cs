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

        internal override IEnumerable<IssueBase> DirectIssues => Issue.plus(base.DirectIssues);

        public CompileSyntaxError
            (
            IssueId issueId,
            IToken token,
            CompileSyntaxError previous = null)
            : base(token)
        {
            _issueId = issueId;
            _previous = previous;
            _issueCache = new ValueCache<CompileSyntaxIssue>
                (() => new CompileSyntaxIssue(_issueId, Token));
        }

        protected override IEnumerable<Syntax> DirectChildren()
        {
            yield return _previous;
        }

        [DisableDump]
        CompileSyntaxIssue Issue => _issueCache.Value;

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

        IssueType IssueType(ContextBase context) => new IssueType(Issue, context.RootContext);

        internal override bool IsError => true;
        internal override Syntax SyntaxError
            (IssueId issue, IToken token, Syntax right = null)
            => new CompileSyntaxError(issue, token, this);
    }
}