using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Scanner;
using Reni.Parser;
using Reni.TokenClasses.Whitespace.Comment;

namespace Reni.TokenClasses.Whitespace
{
    sealed class LineCommentType : DumpableObject, IItemsType, ILine
    {
        internal sealed class HeadType : DumpableObject, Comment.Line.IHead
        {
            internal static readonly HeadType Instance = new();
            bool IItemType.IsSeparatorRequired => false;
        }

        internal sealed class TextLineType : DumpableObject, Comment.Line.IText
        {
            internal static readonly TextLineType Instance = new();
            bool IItemType.IsSeparatorRequired => false;
        }

        internal sealed class TailType : DumpableObject, Comment.Line.ITail
        {
            internal static readonly TailType Instance = new();
            bool IItemType.IsSeparatorRequired => false;
        }

        internal static readonly LineCommentType Instance = new();

        IEnumerable<WhiteSpaceItem> IItemsType.GetItems(SourcePart sourcePart, IParent parent)
        {
            var headLength = sourcePart.Start.Match(Lexer.Instance.LineCommentHead);
            headLength.AssertIsNotNull();
            var head = sourcePart.Start.Span(headLength.Value);

            var tailLength = sourcePart.End.Match(Lexer.Instance.LineEnd, false);
            tailLength.AssertIsNotNull();
            var tail = sourcePart.End.Span(tailLength.Value);

            yield return new(HeadType.Instance, head, parent);
            yield return new(TextLineType.Instance, head.End.Span(tail.Start), parent);
            yield return new(TailType.Instance, tail, parent);
        }
        bool IItemType.IsSeparatorRequired => false;
    }
}