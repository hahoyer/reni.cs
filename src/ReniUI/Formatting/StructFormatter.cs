using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;

namespace ReniUI.Formatting
{
    public sealed class StructFormatter : DumpableObject, IFormatter
    {
        internal readonly Configuration Configuration;
        public StructFormatter(Configuration configuration) => Configuration = configuration;

        IEnumerable<Edit> IFormatter.GetEditPieces(CompilerBrowser compiler, SourcePart targetPart)
        {

            var sourcePartEdits = structItem.GetSourcePartEdits(targetPart).ToArray();
            var editPieces = sourcePartEdits.GetEditPieces(targetPart, Configuration);
            return editPieces;
        }
    }

    interface ISourcePartEdit {}
}