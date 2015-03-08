using System.Linq;
using System.Collections.Generic;
using System;
using hw.Debug;
using hw.Parser;
using Reni.Code;
using Reni.Parser;

namespace Reni.Validation
{
    sealed class ConsequentialError : SyntaxIssue
    {
        [EnableDump]
        readonly IssueBase _issueBase;

        public ConsequentialError(Token token, IssueBase issueBase)
            : base(token, IssueId.ConsequentialError) { _issueBase = issueBase; }
        internal override CodeBase Code => _issueBase.Code + base.Code;
    }
}