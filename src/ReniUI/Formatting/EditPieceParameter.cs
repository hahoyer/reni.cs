using System;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;

namespace ReniUI.Formatting
{
    sealed class EditPieceParameter : DumpableObject
    {
        readonly Configuration Configuration;
        public int Indent;
        public int LineBreakCount;
        public int SpaceCount;

        public EditPieceParameter(Configuration configuration) => Configuration = configuration;

        Edit Create(SourcePosn span)
        {
            var lineBreakText = "\n".Repeat(LineBreakCount);
            var indentText = LineBreakCount > 0 ? " ".Repeat(Indent * Configuration.IndentCount) : "";
            var spaceText = " ".Repeat(SpaceCount);

            return new Edit
            {
                Location = span.Span(0),
                NewText = lineBreakText + indentText + spaceText
            };
        }

        public void Reset()
        {
            LineBreakCount = 0;
            SpaceCount = 0;
        }

        internal Edit GetEditPiece
            (bool hasCommentLineBreak, int[] currentLineBreaks, SourcePosn anchor, int[] currentSpaces)
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
            var newspacesCount = SpaceCount;
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
            var newLineBreakCount = LineBreakCount;
            if(hasCommentLineBreak)
                newLineBreakCount--;

            if(Configuration.EmptyLineLimit == null)
                newLineBreakCount = currentLineBreaks.Length;
            else
                newLineBreakCount = Math.Max
                (
                    newLineBreakCount,
                    Math.Min
                    (
                        Configuration.EmptyLineLimit.Value,
                        currentLineBreaks.Length
                    )
                );

            var delta = newLineBreakCount - currentLineBreaks.Length;

            return
                (
                delta < 0 ? currentLineBreaks[newLineBreakCount] : 0,
                delta > 0 ? "\n".Repeat(delta) : ""
                );
        }
    }
}