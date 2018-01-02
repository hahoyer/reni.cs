using hw.DebugFormatter;

namespace ReniUI.Formatting
{
    sealed class SourcePartEdit : DumpableObject, ISourcePartEdit
    {
        [EnableDump]
        internal FormatterToken Source;

        internal SourcePartEdit(FormatterToken source)
        {
            Source = source;
            StopByObjectIds(3161,3169);
        }

        internal Edit GetEditPiece(EditPieceParameter parameter)
            => Source.GetEditPiece(parameter);

        protected override string GetNodeDump() {return base.GetNodeDump() + " " + Source.OrientationDump;}
    }
}