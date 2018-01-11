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
        public bool IsSpaceRequired;
        public int LineBreakCount;

        public EditPieceParameter(Configuration configuration) => Configuration = configuration;

        Edit Create(SourcePosn span)
        {
            var lineBreakText = "\n".Repeat(LineBreakCount);
            var indentText = LineBreakCount > 0 ? " ".Repeat(Indent * Configuration.IndentCount) : "";
            var spaceText = IsSpaceRequired ? " " : "";

            return new Edit
            {
                Location = span.Span(0),
                NewText = lineBreakText + indentText + spaceText
            };
        }

        public void Reset()
        {
            LineBreakCount = 0;
            IsSpaceRequired = false;
        }

        internal Edit 
            GetEditPiece(bool hasCommentLineBreak, int[] currentLineBreaks, SourcePosn anchor, int[] currentSpaces)
        {
            var lineBreakPart = GetLineBreakPart(hasCommentLineBreak, currentLineBreaks);
            var spacesPart = GetSpacesPart(currentSpaces);

            var location = (anchor + lineBreakPart.startPosition).Span(anchor + spacesPart.endPosition);
            var newText = lineBreakPart.text + spacesPart.text;

            if(location.Length == 0 && newText == "")
                return null;

            return
                new Edit
                {
                    Location = location,
                    NewText = newText
                };
        }

        (string text, int endPosition) GetSpacesPart(int[] currentSpaces)
        {
            var newspacesCount = IsSpaceRequired ? 1 : 0;
            if(LineBreakCount > 0)
                newspacesCount += Indent * Configuration.IndentCount;

            var delta = newspacesCount - currentSpaces.Length;

            return
                (
                delta > 0 ? " ".Repeat(delta) : "",
                delta < 0 ? currentSpaces[-delta - 1] : 0
                );
        }

        (int startPosition, string text) GetLineBreakPart(bool hasCommentLineBreak, int[] currentLineBreaks)
        {
            var newLineBreakCount = NewLineBreakCount(hasCommentLineBreak, currentLineBreaks.Length);

            var delta = newLineBreakCount - currentLineBreaks.Length;

            return
                (
                delta < 0 ? currentLineBreaks[newLineBreakCount] : 0,
                delta > 0 ? "\n".Repeat(delta) : ""
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