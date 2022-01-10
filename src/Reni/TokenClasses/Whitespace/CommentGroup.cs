using System.Collections.Generic;
using hw.DebugFormatter;
using Reni.TokenClasses.Whitespace.Comment;

namespace Reni.TokenClasses.Whitespace;

sealed class CommentGroup : DumpableObject, LinesAndSpaces.IPredecessor
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

    bool LinesAndSpaces.IPredecessor.IsLineComment => Comment.Type is ILine;

    internal IEnumerable<Edit> GetEdits(int indent) => Head.GetEdits(indent);
}