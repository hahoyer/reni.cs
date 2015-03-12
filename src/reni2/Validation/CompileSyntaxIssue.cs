using System.Linq;
using System.Collections.Generic;
using System;
using hw.Debug;
using hw.Parser;
using hw.Scanner;

namespace Reni.Validation
{
    sealed class CompileSyntaxIssue : IssueBase
    {
        readonly IToken _tokenData;

        internal CompileSyntaxIssue(IssueId issueId, IToken tokenData)
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