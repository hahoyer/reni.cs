using System.Collections.Generic;
using hw.DebugFormatter;

namespace Reni.TokenClasses.Whitespace;

sealed class CommentGroup : DumpableObject
{
    [EnableDump]
    internal readonly WhiteSpaceItem Comment;

    [EnableDump]
    readonly LinesAndSpaces Head;

    internal CommentGroup(LinesAndSpaces head, WhiteSpaceItem comment)
    {
        Head = head;
        Comment = comment;
    }

    internal IEnumerable<Edit> GetEdits(int indent) => Head.GetEdits(indent);
}