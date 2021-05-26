using System;
using hw.DebugFormatter;
using hw.Scanner;

namespace Reni.Validation
{
    [Obsolete("",true)]
    public sealed class SourceIssue : DumpableObject
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

        internal IssueId IssueId => Issue.IssueId;
    }
}