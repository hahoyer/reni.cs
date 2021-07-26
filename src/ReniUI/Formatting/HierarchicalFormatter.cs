using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;

namespace ReniUI.Formatting
{
    sealed class HierarchicalFormatter : DumpableObject, IFormatter
    {
        readonly Configuration Configuration;

        public HierarchicalFormatter(Configuration configuration) => Configuration = configuration;

        IEnumerable<Edit> IFormatter.GetEditPieces(CompilerBrowser compilerBrowser, SourcePart targetPart)
        {
            if(compilerBrowser.IsTooSmall(targetPart))
                return new Edit[0];

            var item = Syntax.Create(compilerBrowser.Syntax.FlatItem, Configuration);
            //item.t();
            item.SetupLineBreaks();
            //item.LogDump().Log(FilePositionTag.Debug);
            var sourcePartEdits = item.Edits.ToArray();
            var editPieces = sourcePartEdits
                .GetEditPieces(Configuration)
                .Where(editPiece=> IsRelevant(editPiece, targetPart))
                .ToArray();
            //editPieces.LogDump().Log(FilePositionTag.Debug);
            return editPieces;
        }

        static bool IsRelevant(Edit editPiece, SourcePart targetPart)
        {
            if(targetPart == null)
                return true;
            var sourcePart = editPiece.Location;
            return targetPart.Source == sourcePart.Source
                && targetPart.Position <= sourcePart.EndPosition 
                && sourcePart.Position <= targetPart.EndPosition;
        }
    }
}