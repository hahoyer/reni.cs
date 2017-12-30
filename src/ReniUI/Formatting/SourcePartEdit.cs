using hw.DebugFormatter;

namespace ReniUI.Formatting
{
    sealed class SourcePartEdit : DumpableObject, ISourcePartEdit
    {
        [EnableDump]
        FormatterToken Source;

        internal SourcePartEdit(FormatterToken source) => Source = source;

        internal Edit GetEditPiece(EditPieceParameter parameter)
            => Source.GetEditPiece(parameter);
    }
}