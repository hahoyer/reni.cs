using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Parser;
using hw.Scanner;
using Reni.Validation;

namespace Reni.Parser
{
    abstract class SyntaxIssue : IssueBase
    {
        [EnableDump]
        readonly IToken _token;

        internal SyntaxIssue(IToken token, IssueId issueId)
            : base(issueId) { _token = token; }

        internal override string LogDump => _token.Characters.FileErrorPosition(Tag);
    }
}