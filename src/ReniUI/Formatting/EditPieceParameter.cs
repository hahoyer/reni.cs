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

        public IEnumerable<Edit> GetEditPieces(SourcePosn sourcePosn, int currentLineBreakCount, int currentSpaceCount)
        {
            if(currentLineBreakCount == LineBreakCount && currentSpaceCount == SpaceCount && Indent == 0)
                return new Edit[0];

            if(currentLineBreakCount == 0 && currentSpaceCount == 0)
                return new[] {Create(sourcePosn)};

            NotImplementedMethod(sourcePosn, currentLineBreakCount, currentSpaceCount);
            return null;
        }
    }
}