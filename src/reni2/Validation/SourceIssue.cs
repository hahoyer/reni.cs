using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;

namespace Reni.Validation
{
    [Obsolete("",true)]
    public sealed class SourceIssue : DumpableObject, ISourcePartProxy
    {
        internal SourceIssue(Issue issue, SourcePart sourcePart)
        {
            Issue = issue;
            SourcePart = sourcePart;
            StopByObjectIds(35);
        }

        internal Issue Issue { get; }
        SourcePart SourcePart { get; }

        [DisableDump]
        internal string GetLogDump => Issue.LogDump;

        SourcePart ISourcePartProxy.All => SourcePart;
        internal IssueId IssueId => Issue.IssueId;
    }
}