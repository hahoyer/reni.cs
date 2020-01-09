using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;

namespace ReniUI.Formatting
{
    sealed class EditPieceParameter : DumpableObject
    {
        readonly Configuration Configuration;
        public int Indent;

        public bool IsEndOfFile;
        public bool IsSeparatorRequired;
        public int LineBreakCount;

        public EditPieceParameter(Configuration configuration) => Configuration = configuration;

        int IndentCharacterCount
        {
            get
            {
                if(Indent > 0)
                    return Indent * Configuration.IndentCount;
                return 0;
            }
        }

        public void Reset()
        {
            LineBreakCount = 0;
            IsSeparatorRequired = false;
        }

        internal IEnumerable<Edit> GetEditPiece
        (
            bool hasCommentLineBreak,
            SourcePart[] currentLineBreaks,
            SourcePosn anchor,
            int[] currentSpaces
        )
        {
            var lineBreakPart = GetLineBreakPart(hasCommentLineBreak, currentLineBreaks, anchor);
            var spacesPart = GetSpacesPart(currentSpaces, lineBreakPart.isSeparatorRequired);

            var location = (anchor + lineBreakPart.startPosition).Span(anchor + spacesPart.endPosition);
            var newText = lineBreakPart.text + spacesPart.text;

            if(location.Length == 0 && newText == "")
                return Enumerable.Empty<Edit>();

            var piece = new List<Edit>();
            foreach(var lineBreak in currentLineBreaks.Where(item => item.Length > 0 && item.End <= location.Start))
                piece.Add
                (
                    new Edit {Location = lineBreak, NewText = ""});
            piece.Add
            (
                new Edit {Location = location, NewText = newText});
            return piece;
        }

        (string text, int endPosition) GetSpacesPart(int[] currentSpaces, bool isSeparatorRequired)
        {
            var newSpacesCount = isSeparatorRequired ? 1 : 0;
            if(LineBreakCount > 0)
                newSpacesCount += IndentCharacterCount;

            var delta = newSpacesCount - currentSpaces.Length;

            return
            (
                delta > 0 ? " ".Repeat(delta) : "",
                delta < 0 ? currentSpaces[-delta - 1] : 0
            );
        }

        (int startPosition, string text, bool isSeparatorRequired) GetLineBreakPart
            (bool hasCommentLineBreak, SourcePart[] currentLineBreaks, SourcePosn anchor)
        {
            var newLineBreakCount = NewLineBreakCount(hasCommentLineBreak, currentLineBreaks.Length);

            var delta = newLineBreakCount - currentLineBreaks.Length;

            return
            (
                delta < 0 ? currentLineBreaks[newLineBreakCount].End - anchor : 0,
                delta > 0 ? "\n".Repeat(delta) : "",
                newLineBreakCount == 0 && IsSeparatorRequired
            );
        }

        int NewLineBreakCount(bool hasCommentLineBreak, int currentLineBreaks)
        {
            if(IsEndOfFile && Configuration.LineBreakAtEndOfText != null)
                return Configuration.LineBreakAtEndOfText.Value ? 1 : 0;

            var effectiveLineBreakCount = currentLineBreaks;

            var emptyLineLimit = Configuration.EmptyLineLimit;

            if(emptyLineLimit != null && effectiveLineBreakCount > emptyLineLimit.Value)
                effectiveLineBreakCount = emptyLineLimit.Value;

            var result = LineBreakCount - (hasCommentLineBreak ? 1 : 0);
            return result > effectiveLineBreakCount ? result : effectiveLineBreakCount;
        }
    }
}