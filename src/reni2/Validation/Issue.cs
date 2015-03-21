using System.Linq;
using System.Collections.Generic;
using System;
using hw.Debug;
using hw.Helper;
using hw.Scanner;
using JetBrains.Annotations;
using Reni.Code;
using Reni.ReniParser;

namespace Reni.Validation
{
    sealed class Issue : DumpableObject
    {
        internal static readonly IEnumerable<Issue> Empty = new Issue[0];

        internal Issue(IssueId issueId, [NotNull] Syntax source, string message = null)
        {
            IssueId = issueId;
            Source = source;
            Message = message;
            AssertValid();
        }
        void AssertValid() { Tracer.Assert(Source != null); }

        internal IssueId IssueId { get; }
        internal Syntax Source { get; }
        string Message { get; }

        string Tag => IssueId.Tag;

        internal CodeBase Code => CodeBase.Issue(this);

        protected override string GetNodeDump()
            => base.GetNodeDump() + IssueId.NodeDump().Surround("{", "}");

        internal string GetLogDump(SourcePart position)
        {
            var result = position.FileErrorPosition(Tag);
            if(string.IsNullOrWhiteSpace(Message))
                return result;
            return result + " " + Message;
        }
    }
}