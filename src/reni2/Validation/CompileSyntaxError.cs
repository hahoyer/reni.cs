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

        public CompileSyntaxError
            (IssueId issueId, SourcePart token, SourcePart sourcePart = null, CompileSyntaxError previous = null)
            : base(sourcePart + token + previous?.SourcePart, token)
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
        public override CompileSyntax Sourround(SourcePart sourcePart) => this;

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

        internal override Syntax SyntaxError(SourcePart sourcePart, IssueId issue, SourcePart token, Syntax right = null)
            => new CompileSyntaxError(issue, token, sourcePart + right.SourcePart, this);
    }
}