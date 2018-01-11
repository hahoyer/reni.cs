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

            var items = token.PrecededWith.ToArray();
            var resultItem = new FormatterToken();
            resultItem.Anchor = token.SourcePart().Start;

            var result = new List<FormatterToken>();
            foreach(var item in items)
            {
                if(item.IsComment())
                {
                    Tracer.Assert(resultItem.Anchor != null);
                    result.Add(resultItem);
                    resultItem = new FormatterToken();
                }

                resultItem.Modify(item);
            }

            if(returnMain)
            {
                Tracer.Assert(resultItem.Anchor != null);
                result.Add(resultItem);
            }

            return result;
        }

        readonly List<SourcePosn> LineBreaks = new List<SourcePosn>();

        [EnableDump]
        SourcePosn Anchor;

        [EnableDumpExcept(false)]
        bool HasCommentLineBreak;

        List<SourcePart> Spaces = new List<SourcePart>();

        SourcePart ISourcePartProxy.All
            => (Anchor + (RelativeLineBreakPositions.Any() ? RelativeLineBreakPositions.First() : 0))
                .Span(Anchor + (RelativeSpacePositions.Any() ? RelativeSpacePositions.Last() : 0));

        [EnableDump]
        int[] RelativeLineBreakPositions => LineBreaks.Select(index => index - Anchor).ToArray();

        [EnableDump]
        int[] RelativeSpacePositions => Spaces.Select(part => part.End - Anchor).ToArray();

        internal bool IsEmpty =>
            !HasCommentLineBreak && !RelativeLineBreakPositions.Any() && !RelativeSpacePositions.Any();

        internal string OrientationDump => Anchor.GetDumpAroundCurrent(5);

        void Modify(IItem item)
        {
            if(item.IsComment())
                HasCommentLineBreak = Lexer.IsLineComment(item);

            if(item.IsWhiteSpace())
            {
                if(Spaces.Any())
                    Tracer.Assert(Spaces.Last().End == item.SourcePart.Start);
                Spaces.Add(item.SourcePart);
            }

            if(item.IsLineBreak())
            {
                if(Spaces.Any())
                    Tracer.Assert(Spaces.Last().End == item.SourcePart.Start);

                LineBreaks.Add(Spaces.Any() ? Spaces.First().Start : item.SourcePart.Start);
                Spaces = new List<SourcePart>();
            }

            if(item.IsComment() || item.IsLineBreak())
                Anchor = item.SourcePart.End;
        }

        internal ISourcePartEdit ToSourcePartEdit() => new SourcePartEdit(this);

        internal Edit GetEditPiece(EditPieceParameter parameter)
            => parameter.GetEditPiece
            (
                HasCommentLineBreak,
                RelativeLineBreakPositions,
                Anchor,
                RelativeSpacePositions
            );
    }
}