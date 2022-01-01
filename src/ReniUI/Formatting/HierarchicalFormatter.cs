using System;
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

            var item = SyntaxTreeProxy.Create(compilerBrowser.Syntax.FlatItem, Configuration);
            var trace = DateTime.Today.Year > 2020;
            if(trace) item.t();
            item.SetupLineBreaks();
            if(trace) item.LogDump().Log(FilePositionTag.Debug);
            var sourcePartEdits = item.Edits.ToArray();
            var editPieces
                = sourcePartEdits
                    .GetEditPieces()
                    .Where(editPiece => IsRelevant(editPiece, targetPart))
                    .ToArray();
            if(trace) editPieces.LogDump().Log(FilePositionTag.Debug);
            return editPieces;
        }

        static bool IsRelevant(Edit editPiece, SourcePart targetPart)
        {
            if(targetPart == null)
                return true;
            var sourcePart = editPiece.Remove;
            return targetPart.Source == sourcePart.Source &&
                targetPart.Position <= sourcePart.EndPosition &&
                sourcePart.Position <= targetPart.EndPosition;
        }
    }
}