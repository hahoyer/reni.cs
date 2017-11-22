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
        public int Space;

        public EditPieceParameter(Configuration configuration) => Configuration = configuration;

        Edit Create(SourcePosn span)
        {
            var lineBreakText = "\n".Repeat(LineBreakCount);
            var indentText = LineBreakCount > 0 ? " ".Repeat(Indent * Configuration.IndentCount) : "";
            var spaceText = " ".Repeat(Space);

            return new Edit
            {
                Location = span.Span(0),
                NewText = lineBreakText + indentText + spaceText
            };
        }

        public void Reset()
        {
            LineBreakCount = 0;
            Space = 0;
        }

        public IEnumerable<Edit> GetEditPieces(SourcePosn sourcePosn, int lineBreakCount, int spaceCount)
        {
            if(lineBreakCount == 0 && spaceCount == 0)
            {
                if(LineBreakCount == 0 && Space == 0)
                    return new Edit[0];
                return new[] {Create(sourcePosn)};
            }

            NotImplementedMethod(sourcePosn, lineBreakCount, spaceCount);
            return null;
        }
    }
}