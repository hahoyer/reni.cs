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
        readonly hw.Parser.Token _tokenData;

        internal CompileSyntaxIssue(IssueId issueId, hw.Parser.Token tokenData)
            : base(issueId) { _tokenData = tokenData; }

        internal override string LogDump
        {
            get
            {
                var result = _tokenData.Characters.FileErrorPosition(Tag);
                return result;
            }
        }
    }
}