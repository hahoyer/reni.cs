using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Scanner;

namespace ReniUI.Formatting
{
    public class StructFormatter : DumpableObject, IFormatter
    {
        internal readonly Configuration Configuration;
        public StructFormatter(Configuration configuration) => Configuration = configuration;

        IEnumerable<Edit> IFormatter.GetEditPieces(CompilerBrowser compiler, SourcePart targetPart)
        {
            var structItem = compiler.Locate(targetPart).CreateStruct(this);

            var sourcePartEdits = structItem.GetSourcePartEdits(targetPart);
            var editPieces = sourcePartEdits.GetEditPieces(targetPart, Configuration);
            return editPieces;
        }
    }

    sealed class SourcePartEdit : DumpableObject, ISourcePartEdit
    {
        [EnableDump]
        FormatterToken Source;

        internal SourcePartEdit(FormatterToken source) => Source = source;

        internal IEnumerable<Edit> GetEditPieces(EditPieceParameter parameter) 
            => Source.GetEditPieces(parameter);
    }

    interface ISourcePartEdit {}
}