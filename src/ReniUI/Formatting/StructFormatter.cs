using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;

namespace ReniUI.Formatting
{
    sealed class StructFormatter : DumpableObject, IFormatter
    {
        readonly Configuration Configuration;
        public StructFormatter(Configuration configuration) => Configuration = configuration;

        IEnumerable<Edit> IFormatter.GetEditPieces(CompilerBrowser compiler, SourcePart targetPart)
        {
            var syntax = compiler.LocateAndFilter(targetPart);
            if(syntax == null)
                return new Edit[0];

            var item = new Structure(compiler.FormattingSyntax, Configuration);

            ISourcePartEdit[] sourcePartEdits = item.Edits.ToArray();
            var editPieces = sourcePartEdits.GetEditPieces(Configuration);
            return editPieces;
        }
    }

    interface ISourcePartEdit {}
}                                                                             