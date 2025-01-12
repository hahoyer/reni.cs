using hw.Scanner;

namespace Reni.TokenClasses.Whitespace;

sealed class WhiteSpaceView : DumpableObject
{
    readonly SourcePart SourcePartForTrace;

    /// <summary>
    ///     List of comment items possibly prefixed by lines and spaces
    /// </summary>
    [EnableDump]
    readonly CommentGroup[] Comments;

    /// <summary>
    ///     Lines and spaces that prefix the main token
    /// </summary>
    [EnableDump]
    readonly LineGroup Tail;


    internal WhiteSpaceView(CommentGroup[] comments, LineGroup tail, SourcePart sourcePartForTrace)
    {
        Comments = comments ?? new CommentGroup[0];
        Tail = tail;
        SourcePartForTrace = sourcePartForTrace;
    }

    protected override string GetNodeDump() => SourcePartForTrace.NodeDump + " " + base.GetNodeDump();

    internal IEnumerable<Edit> GetEdits(int indent)
    {
        var commentEdits = Comments
            .SelectMany((item, index) => item.GetEdits(indent)
                .ToArray());

        var linesAndSpacesEdits
            = Tail.GetEdits(indent).ToArray();

        return T(commentEdits, linesAndSpacesEdits).ConcatMany();
    }
}