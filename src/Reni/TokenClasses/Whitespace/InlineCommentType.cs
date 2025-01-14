using hw.Scanner;
using Reni.Parser;
using Reni.TokenClasses.Whitespace.Comment;
using Reni.TokenClasses.Whitespace.Comment.Inline;

namespace Reni.TokenClasses.Whitespace;

sealed class InlineCommentType : DumpableObject, IInline, IItemsType
{
    internal sealed class HeadType : DumpableObject, IHead
    {
        internal static readonly HeadType Instance = new();
        bool IItemType.IsSeparatorRequired => false;
    }

    internal sealed class IdType : DumpableObject, IIdentifier
    {
        internal static readonly IdType Instance = new();
        bool IItemType.IsSeparatorRequired => false;
    }

    sealed class TextType : VariantListType, IText
    {
        internal sealed class TextLineType : DumpableObject, Comment.Inline.Text.IText
        {
            internal static readonly TextLineType Instance = new();
            bool IItemType.IsSeparatorRequired => false;
        }

        internal sealed class LineEndType : DumpableObject, Comment.Inline.Text.ILineBreak
        {
            internal static readonly LineEndType Instance = new();
            bool IItemType.IsSeparatorRequired => false;
        }

        internal static readonly TextType Instance = new();

        [DisableDump]
        protected override ItemPrototype[] VariantPrototypes { get; } =
        {
            new(LineEndType.Instance, Lexer.Instance.LineEnd)
            , new(TextLineType.Instance, Lexer.Instance.LineEndOrEnd.Until)
        };
    }

    sealed class TailType : DumpableObject, ITail
    {
        internal static readonly TailType Instance = new();
        bool IItemType.IsSeparatorRequired => true;
    }

    internal static readonly InlineCommentType Instance = new();

    IEnumerable<WhiteSpaceItem> IItemsType.GetItems(SourcePart sourcePart, IParent? parent)
    {
        var headLength = sourcePart.Start.Match(Lexer.Instance.InlineCommentHead);
        headLength.AssertIsNotNull();
        var head = sourcePart.Start.Span(headLength!.Value);
        yield return new(HeadType.Instance, head, parent);

        var tailLength = sourcePart.Match(Lexer.Instance.InlineCommentTail, false);
        tailLength.AssertIsNotNull();
        var tail = sourcePart.End.Span(tailLength!.Value);

        var contentWithNames = GetContentWithNames(head.End.Span(tail.Start), parent);
        foreach(var item in contentWithNames)
            yield return item;

        yield return new(TailType.Instance, tail, parent);
    }

    bool IItemType.IsSeparatorRequired => true;

    static IEnumerable<WhiteSpaceItem> GetContentWithNames(SourcePart sourcePart, IParent? parent)
    {
        if(sourcePart.Length == 0)
        {
            yield return new(TextType.Instance, sourcePart, parent);
            yield break;
        }

        var beforeWhiteSpace = sourcePart.Match(Match.WhiteSpace.Until);
        if(beforeWhiteSpace == null)
        {
            yield return new(TextType.Instance, sourcePart, parent);
            yield break;
        }

        var open = sourcePart.Start.Span(beforeWhiteSpace.Value);

        var afterWhiteSpace = sourcePart.Match(Match.WhiteSpace.Until, false);
        afterWhiteSpace.AssertIsNotNull();
        var close = sourcePart.End.Span(afterWhiteSpace!.Value);

        (open.Id == close.Id).Assert();
        if(open.Id.Length > 0)
            yield return new(IdType.Instance, open, parent);

        yield return new(TextType.Instance, open.End.Span(close.Start), parent);

        if(close.Id.Length > 0)
            yield return new(IdType.Instance, close, parent);
    }
}