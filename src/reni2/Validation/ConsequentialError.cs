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

        public ConsequentialError(TokenData position, IssueBase issueBase)
            : base(position, IssueId.ConsequentialError) { _issueBase = issueBase; }
        internal override CodeBase Code { get { return _issueBase.Code + base.Code; } }
    }
}