using hw.Scanner;

namespace Reni.TokenClasses.Whitespace;

static class Extension
{
    static( (WhiteSpaceItem[] Head, WhiteSpaceItem? Main)[] Items, WhiteSpaceItem[] Tail) SplitAndTail
        (this IEnumerable<WhiteSpaceItem> items, Func<WhiteSpaceItem, bool> tailCondition)
    {
        items.AssertIsNotNull();
        if(!items.Any())
            return ([], []);

        var result = items.Split(tailCondition, LinqExtension.SeparatorTreatmentForSplit.EndOfSubList)
            .Select(items => items.ToArray().SplitMore(tailCondition))
            .ToArray()
            .SplitMore(item => item.Item2 == default);

        var tail = result.Item2.Item1 ?? [];
        return (Items: result.Item1, Tail: tail);
    }

    static(TItem[], TItem?) SplitMore<TItem>(this TItem[] items, Func<TItem, bool> tailCondition)
    {
        var last = items.Last();
        return tailCondition(last)? (items.Take(items.Length - 1).ToArray(), last) : (items, default);
    }

    static
        ((((WhiteSpaceItem[] Head, WhiteSpaceItem? Main)[] Items, WhiteSpaceItem[] Tail) Lines, WhiteSpaceItem? Main)[]
        Comments
        , ((WhiteSpaceItem[] Head, WhiteSpaceItem? Main)[] Items, WhiteSpaceItem[] Tail) TailLines)
        SplitAndTail(IEnumerable<WhiteSpaceItem> allItems)
    {
        var groups = allItems.SplitAndTail(CommentTailCondition);
        var comments = groups.Items.Select(item => (Lines: item.Head.SplitAndTail(LineBreakTailCondition), item.Main))
            .ToArray();
        var tailLines = groups.Tail.SplitAndTail(LineBreakTailCondition);
        return (comments, tailLines);
    }


    [UsedImplicitly]
    static TValue[] T<TValue>(params TValue[] value) => value;

    static bool CommentTailCondition(WhiteSpaceItem item) => item.Type is IComment;
    static bool LineBreakTailCondition(WhiteSpaceItem item) => item.Type is IVolatileLineBreak;

    internal static(CommentGroup[], LineGroup) CreateCommentGroups
    (
        this IEnumerable<WhiteSpaceItem> allItems
        , SourcePosition anchor
        , LineGroup.IConfiguration configuration
    )
    {
        IItemType? predecessor = null;
        var groups = SplitAndTail(allItems);

        var commentGroups
            = groups
                .Comments
                .Select(items => items.CreateCommentGroup(configuration, ref predecessor))
                .ToArray();

        var linesAndSpaces
            = groups
                .TailLines
                .CreateLinesAndSpaces(configuration, anchor, predecessor, true);

        return (commentGroups, linesAndSpaces);
    }

    static CommentGroup CreateCommentGroup
    (
        this(((WhiteSpaceItem[] Head, WhiteSpaceItem? Main)[] Items, WhiteSpaceItem[] Tail) Lines, WhiteSpaceItem? Main)
            items
        , LineGroup.IConfiguration configuration
        , ref IItemType? predecessor
    )
    {
        var head
            = items
                .Lines
                .CreateLinesAndSpaces(configuration, items.Main!.SourcePart.Start, predecessor, false);
        var commentGroup = new CommentGroup(head, items.Main);
        predecessor = commentGroup.Comment.Type;
        return commentGroup;
    }

    static LineGroup CreateLinesAndSpaces
    (
        this((WhiteSpaceItem[] Head, WhiteSpaceItem? Main)[] Items, WhiteSpaceItem[] Tail) groups
        , LineGroup.IConfiguration configuration
        , SourcePosition anchor
        , IItemType? predecessorCommentType
        , bool isLast
    )
    {
        var spaces = groups.Tail;
        spaces.All(space => space.SourcePart.Length == 1).Assert();

        var lineGroups = groups.Items
            .Select(item => item.Main!.GetLineGroup(item.Head.Length))
            .ToArray();

        var sourcePart = (lineGroups.FirstOrDefault()?.Start ?? anchor - spaces.Length).Span(anchor);
        return new(sourcePart, predecessorCommentType, lineGroups.Length, isLast, configuration);
    }

    static SourcePart GetLineGroup(this WhiteSpaceItem lineBreak, int spaceCount)
        => (lineBreak.SourcePart.Start - spaceCount)
            .Span(lineBreak.SourcePart.End);
}