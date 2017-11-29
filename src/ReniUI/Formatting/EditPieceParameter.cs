using System.Collections.Generic;
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

        internal Edit GetEditPiece(int[] currentLineBreaks, SourcePosn anchor, int[] currentSpaces)
        {
            var lineBreakPart = GetLineBreakPart(currentLineBreaks, anchor);
            var spacesPart = GetSpacesPart(anchor, currentSpaces);

            var location = lineBreakPart.start.Span(spacesPart.end);
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

        (string text, SourcePosn end) GetSpacesPart(SourcePosn anchor, int[] currentSpaces)
        {
            var newspacesCount = SpaceCount;
            if(LineBreakCount > 0)
                newspacesCount += Indent * Configuration.IndentCount;

            var spaceDelta = newspacesCount - currentSpaces.Length;

            return spaceDelta > 0
                ? (" ".Repeat(spaceDelta), anchor)
                : spaceDelta == 0
                    ? ("", anchor)
                    : ("", anchor.Source + currentSpaces[-spaceDelta - 1]);
        }

        (SourcePosn start, string text) GetLineBreakPart(int[] currentLineBreaks, SourcePosn anchor)
        {
            var lineBreaksDelta = LineBreakCount - currentLineBreaks.Length;

            if(lineBreaksDelta > 0)
                return (anchor, "\n".Repeat(lineBreaksDelta));
            if(lineBreaksDelta == 0)
                return (anchor, "");
            return (anchor.Source + currentLineBreaks[-lineBreaksDelta - 1], "");
        }
    }
}