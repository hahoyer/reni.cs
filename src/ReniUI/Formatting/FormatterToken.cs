using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using hw.Scanner;
using Reni.Parser;

namespace ReniUI.Formatting
{
    sealed class FormatterToken : DumpableObject, ISourcePartProxy
    {
        internal static IEnumerable<FormatterToken> Create(IToken token, bool returnMain = true)
        {
            if(token == null)
                return Enumerable.Empty<FormatterToken>();

            var anchor = token.SourcePart().Start;
            var items = token.PrecededWith.ToArray();
            var hasCommentLineBreak = false;
            var lineBreaks = new List<SourcePosn>();
            var spaces = new List<SourcePosn>();

            var result = new List<FormatterToken>();
            foreach(var item in items)
                if(item.IsComment())
                {
                    result.Add(new FormatterToken(hasCommentLineBreak, lineBreaks, anchor, spaces));
                    hasCommentLineBreak = Lexer.IsLineComment(item);
                    lineBreaks = new List<SourcePosn>();
                    spaces = new List<SourcePosn>();
                    anchor = item.SourcePart.End;
                }
                else if(item.IsWhiteSpace())
                    spaces.Add(item.SourcePart.End);
                else
                {
                    Tracer.Assert(item.IsLineBreak());
                    lineBreaks.Add(item.SourcePart.Start);
                    spaces = new List<SourcePosn>();
                    anchor = item.SourcePart.End;
                }

            if(returnMain)
                result.Add(new FormatterToken(hasCommentLineBreak, lineBreaks, anchor, spaces));
            return result;
        }

        [EnableDump]
        readonly SourcePosn Anchor;

        [EnableDumpExcept(false)]
        readonly bool HasCommentLineBreak;

        [EnableDump]
        readonly int[] RelativeLineBreakPositions;

        [EnableDump]
        readonly int[] RelativeSpacePositions;

        FormatterToken
        (
            bool hasCommentLineBreak,
            IEnumerable<SourcePosn> lineBreaks,
            SourcePosn anchor,
            IEnumerable<SourcePosn> spaces)
        {
            HasCommentLineBreak = hasCommentLineBreak;
            Anchor = anchor;
            RelativeLineBreakPositions = lineBreaks.Select(index => index - anchor).ToArray();
            RelativeSpacePositions = spaces.Select(index => index - anchor).ToArray();
        }

        SourcePart ISourcePartProxy.All
            => (Anchor + (RelativeLineBreakPositions.Any() ? RelativeLineBreakPositions.First() : 0))
                .Span(Anchor + (RelativeSpacePositions.Any() ? RelativeSpacePositions.Last() : 0));

        internal bool IsEmpty =>
            !HasCommentLineBreak && !RelativeLineBreakPositions.Any() && !RelativeSpacePositions.Any();

        internal string OrientationDump => Anchor.GetDumpAroundCurrent(5);

        internal SourcePartEdit ToSourcePartEdit() => new SourcePartEdit(this);

        internal Edit GetEditPiece(EditPieceParameter parameter)
            => parameter.GetEditPiece(HasCommentLineBreak, RelativeLineBreakPositions, Anchor, RelativeSpacePositions);
    }
}