using System.Collections.Generic;
using hw.DebugFormatter;

namespace ReniUI.Formatting
{
    sealed class SourcePartEdit : DumpableObject, ISourcePartEdit
    {
        [EnableDump]
        FormatterToken Source;

        internal SourcePartEdit(FormatterToken source) => Source = source;

        internal IEnumerable<Edit> GetEditPieces(EditPieceParameter parameter)
            => Source.GetEditPieces(parameter);
    }
}