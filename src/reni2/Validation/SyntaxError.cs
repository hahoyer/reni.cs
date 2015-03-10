using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using hw.Parser;
using Reni.ReniParser;

namespace Reni.Validation
{
    sealed class SyntaxError : Syntax
    {
        [EnableDump]
        readonly IssueId _issueId;
        readonly SyntaxError _previous;
        readonly ValueCache<CompileSyntaxIssue> _issueCache;

        public SyntaxError
            (
            IssueId issueId,
            Token token,
            SyntaxError previous = null)
            : base(token)
        {
            _issueId = issueId;
            _previous = previous;
        }

        SyntaxError(SyntaxError other, ParsedSyntax[] parts)
            : base(other, parts)
        {
            _issueId = other._issueId;
            _previous = other._previous;
            _issueCache = new ValueCache<CompileSyntaxIssue>
                (() => new CompileSyntaxIssue(_issueId, Token));
        }

        internal override IEnumerable<IssueBase> Issues => _issueCache.Value.plus(base.Issues);

        internal override bool IsError => true;

        internal override Syntax Surround(params ParsedSyntax[] parts)
            => new SyntaxError(this, parts);

        [DisableDump]
        protected override IEnumerable<Syntax> SyntaxChildren { get { yield return _previous; } }
    }
}