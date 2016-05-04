using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni.Code;
using Reni.TokenClasses;

namespace Reni.Validation
{
    public sealed class Issue : DumpableObject
    {
        internal static readonly IEnumerable<Issue> Empty = new Issue[0];

        internal Issue(IssueId issueId, Syntax syntax, string message = null)
        {
            IssueId = issueId;
            Syntax = syntax;
            Message = message??"";
            AssertValid();
            StopByObjectIds(139,145);
        }

        void AssertValid() => Tracer.Assert(Position != null);

        [DisableDump]
        internal IssueId IssueId { get; }
        [DisableDump]
        Syntax Syntax { get; }
        [EnableDump]
        internal SourcePart Position => Syntax.Token.Characters;
        [EnableDump]
        internal string Message { get; }
        [DisableDump]
        internal string Tag => IssueId.Tag;

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