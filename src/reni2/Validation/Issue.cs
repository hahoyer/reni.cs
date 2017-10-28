using System;
using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;

namespace Reni.Validation
{
    public sealed class Issue : DumpableObject, IEquatable<Issue>
    {
        static int NextObjectId;

        internal static readonly IEnumerable<Issue> Empty = new Issue[0];

        [EnableDump]
        internal readonly SourcePart Position;

        internal Issue(IssueId issueId, SourcePart position, string message = null)
            : base(NextObjectId++)
        {
            IssueId = issueId;
            Message = message ?? "";
            Position = position;
            AssertValid();
            StopByObjectIds();
        }

        bool IEquatable<Issue>.Equals(Issue other)
        {
            if(ReferenceEquals(objA: null, objB: other))
                return false;
            if(ReferenceEquals(this, other))
                return true;
            return
                Equals(Position, other.Position) &&
                Equals(IssueId, other.IssueId) &&
                string.Equals(Message, other.Message);
        }

        [DisableDump]
        internal IssueId IssueId { get; }

        [EnableDump]
        internal string Message { get; }

        [DisableDump]
        internal string Tag => IssueId.Tag;

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

        void AssertValid() => Tracer.Assert(Position != null);

        protected override string GetNodeDump()
            => base.GetNodeDump() +
               IssueId.NodeDump().Surround(left: "{", right: "}");

        public override bool Equals(object obj)
        {
            if(ReferenceEquals(objA: null, objB: obj))
                return false;
            if(ReferenceEquals(this, obj))
                return true;
            return obj is Issue issue && Equals(issue);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Position.GetHashCode();
                hashCode = (hashCode * 397) ^ IssueId.GetHashCode();
                hashCode = (hashCode * 397) ^ Message.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(Issue left, Issue right) => Equals(left, right);
        public static bool operator !=(Issue left, Issue right) => !Equals(left, right);
    }
}