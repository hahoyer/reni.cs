using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using hw.Scanner;
using Reni.Parser;


namespace ReniUI.Formatting
{
    sealed class FormatterToken : DumpableObject
    {
        internal static IEnumerable<FormatterToken> Create(IToken token, bool returnMain = true)
        {
            if(token == null)
                yield break;

            var anchor = token.SourcePart().Start;
            var items = token.PrecededWith.ToArray();
            var hasCommentLineBreak = false;
            var lineBreaks = new List<SourcePosn>();
            var spaces = new List<SourcePosn>();

            foreach(var item in items)
            {
                if(item.IsComment() && (lineBreaks.Any() || spaces.Any()))
                {
                    yield return new FormatterToken(hasCommentLineBreak, lineBreaks, anchor, spaces);
                    hasCommentLineBreak = false;
                    lineBreaks = new List<SourcePosn>();
                    spaces = new List<SourcePosn>();
                }

                if(item.IsWhiteSpace())
                    spaces.Add(item.SourcePart.End);
                else
                    anchor = item.SourcePart.End;

                if(item.IsLineBreak())
                {
                    spaces = new List<SourcePosn>();
                    lineBreaks.Add(item.SourcePart.Start);
                }

                if(Lexer.IsLineComment(item))
                    hasCommentLineBreak = true;
            }

            if(returnMain)
                yield return new FormatterToken(hasCommentLineBreak, lineBreaks, anchor, spaces);
        }

        [EnableDump]
        readonly SourcePosn Anchor;

        [EnableDumpExcept(false)]
        readonly bool HasCommentLineBreak;

        [EnableDump]
        readonly int[] LineBreaks;

        [EnableDump]
        readonly int[] Spaces;

        FormatterToken
        (
            bool hasCommentLineBreak,
            IEnumerable<SourcePosn> lineBreaks,
            SourcePosn anchor,
            IEnumerable<SourcePosn> spaces)
        {
            HasCommentLineBreak = hasCommentLineBreak;
            Anchor = anchor;
            LineBreaks = lineBreaks.Select(index => index - anchor).ToArray();
            Spaces = spaces.Select(index => index - anchor).ToArray();
            StopByObjectIds(223, 224);
        }

        internal SourcePartEdit ToSourcePartEdit() => new SourcePartEdit(this);

        internal Edit GetEditPiece(EditPieceParameter parameter)
            => parameter.GetEditPiece(HasCommentLineBreak, LineBreaks, Anchor, Spaces);

        public bool IsEmpty => !HasCommentLineBreak && !LineBreaks.Any() && !Spaces.Any();
    }
}