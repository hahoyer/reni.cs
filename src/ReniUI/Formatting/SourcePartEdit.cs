using hw.DebugFormatter;

namespace ReniUI.Formatting
{
    sealed class SourcePartEdit : DumpableObject, ISourcePartEdit
    {
        [EnableDump]
        internal FormatterToken Source;

        internal SourcePartEdit(FormatterToken source) => Source = source;

        internal Edit GetEditPiece(EditPieceParameter parameter)
            => Source.GetEditPiece(parameter);

        protected override string GetNodeDump() => base.GetNodeDump() + " " + Source.OrientationDump;
    }
}