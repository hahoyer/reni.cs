using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using hw.Parser;
using Reni.ReniParser;
using Reni.ReniSyntax;

namespace Reni.Validation
{
    sealed class SyntaxError : Syntax
    {
        [EnableDump]
        readonly IssueId _issueId;
        readonly SyntaxError _previous;
        readonly ValueCache<CompileSyntaxIssue> _issueCache;

        SyntaxError
            (
            IssueId issueId,
            IToken token,
            SyntaxError previous = null)
            : base(token)
        {
            _issueId = issueId;
            _previous = previous;
            _issueCache = new ValueCache<CompileSyntaxIssue>
                (() => new CompileSyntaxIssue(_issueId, Token));
        }

        protected override IEnumerable<Syntax> DirectChildren { get { yield return _previous; } }

        [DisableDump]
        internal override IEnumerable<IssueBase> DirectIssues
            => _issueCache.Value.plus(base.DirectIssues);

        internal override bool IsError => true;
    }
}