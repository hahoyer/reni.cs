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

            var item = syntax.Target.CreateStruct(Configuration);

            var sourcePartEdits = item.Edits.ToArray();
            var editPieces = sourcePartEdits.GetEditPieces(Configuration);
            return editPieces;
        }

        internal string FlatFormat(Helper.Syntax syntax, bool areEmptyLinesPossible) 
            => syntax.Target.CreateStruct(Configuration).FlatFormat(areEmptyLinesPossible);
    }

    interface ISourcePartEdit {}

}