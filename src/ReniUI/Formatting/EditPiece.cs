using System;
using hw.DebugFormatter;

namespace ReniUI.Formatting
{
    [Obsolete("",true)]
    public sealed class EditPiece : DumpableObject
    {
        public int EndPosition;
        public string NewText;
        public int RemoveCount;
    }
}