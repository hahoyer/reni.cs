using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.Code;
using Reni.Context;

namespace Reni.Validation
{
    sealed class ConsequentialIssueType : IssueType
    {
        readonly IssueType _issueType;

        public ConsequentialIssueType(IssueType issueType, SourcePart source)
            : base(new Issue(IssueId.ConsequentialError, source)) { _issueType = issueType; }

        internal override Root RootContext => _issueType.RootContext;

        internal override string DumpPrintText
            => _issueType.DumpPrintText + "," + base.DumpPrintText;

        internal override CodeBase Code() => _issueType.Code() + base.Code();
    }
}