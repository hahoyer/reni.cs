using System.Linq;
using System.Collections.Generic;
using System;
using hw.Debug;
using hw.Scanner;

namespace Reni.Validation
{
    sealed class CompileSyntaxIssue : IssueBase
    {
        [EnableDump]
        readonly SourcePart _tokenData;

        internal CompileSyntaxIssue(IssueId issueId, SourcePart tokenData)
            : base(issueId) { _tokenData = tokenData; }

        internal override string LogDump
        {
            get
            {
                var result = _tokenData.FileErrorPosition(Tag);
                return result;
            }
        }
    }
}