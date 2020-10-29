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

            var item = new Syntax(compilerBrowser.Syntax.FlatItem, Configuration);

            var sourcePartEdits = item.Edits.ToArray();
            var editPieces = sourcePartEdits.GetEditPieces(Configuration);
            return editPieces;
        }
    }
}