using hw.Scanner;

namespace Stx.DataTypes
{
    sealed class Issue : DataType
    {
        readonly IssueId IssueId;
        readonly SourcePart Position;

        public Issue(IssueId issueId, SourcePart position)
        {
            IssueId = issueId;
            Position = position;
        }

        public override int ByteSize => 0;
    }
}