using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using hw.Parser;
using Reni.Parser;
using Reni.ReniParser;
using Reni.ReniSyntax;

namespace Reni.Validation
{
    sealed class SyntaxError : Syntax
    {
        [EnableDump]
        readonly IssueId _issueId;
        readonly SyntaxError _previous;
        readonly ValueCache<Issue> _issueCache;

        SyntaxError
            (
            IssueId issueId,
            Syntax source, 
            SyntaxError previous = null)
            : base()
        {
            Source = source;
            _issueId = issueId;
            _previous = previous;
            _issueCache = new ValueCache<Issue>
                (() => new Issue(_issueId, Source, ""));
        }

        [EnableDump]
        public Syntax Source { get; set; }

        protected override IEnumerable<Syntax> DirectChildren { get { yield return _previous; } }

        [DisableDump]
        internal override IEnumerable<Issue> DirectIssues
            => _issueCache.Value.plus(base.DirectIssues);

        internal override bool IsError => true;
    }
}