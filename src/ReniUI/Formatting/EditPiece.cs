using hw.DebugFormatter;

namespace ReniUI.Formatting {
    public sealed class EditPiece : DumpableObject
    {
        internal int EndPosition;
        internal int RemoveCount;
        internal string NewText;
    }
}