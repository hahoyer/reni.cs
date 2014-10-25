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
        readonly SourcePart _position;

        internal SyntaxIssue(SourcePart position, IssueId issueId)
            : base(issueId) { _position = position; }

        internal override string LogDump { get { return _position.FileErrorPosition(Tag); } }
    }
}