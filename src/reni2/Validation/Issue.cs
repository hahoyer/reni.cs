using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using hw.Scanner;
using Reni.Code;

namespace Reni.Validation
{
    public sealed class Issue : DumpableObject
    {
        internal static readonly IEnumerable<Issue> Empty = new Issue[0];

        internal Issue(IssueId issueId, SourcePart position, string message = null)
        {
            IssueId = issueId;
            Position = position;
            Message = message;
            AssertValid();
        }

        void AssertValid() => Tracer.Assert(Position != null);

        [DisableDump]
        internal IssueId IssueId { get; }
        [EnableDump]
        internal SourcePart Position { get; }
        [EnableDump]
        string Message { get; }

        string Tag => IssueId.Tag;

        [DisableDump]
        internal CodeBase Code => CodeBase.Issue(this);

        protected override string GetNodeDump()
            => base.GetNodeDump() + IssueId.NodeDump().Surround("{", "}");

        internal string LogDump
        {
            get
            {
                var result = Position.Id == "("
                    ? Position.Start.FilePosn(Tag) + " Functional"
                    : Position.FileErrorPosition(Tag);
                if(string.IsNullOrWhiteSpace(Message))
                    return result;
                return result + " " + Message;
            }
        }
    }
}