using hw.DebugFormatter;

namespace ReniUI.Formatting
{
    sealed class EditPieceParameter : DumpableObject
    {
        readonly Configuration Configuration;
        public int Indent;

        public bool IsEndOfFile;
        public bool IsSeparatorRequired;
        public int LineBreakCount;

        public EditPieceParameter(Configuration configuration) => Configuration = configuration;

        public int IndentCharacterCount => Indent > 0 ? Indent * Configuration.IndentCount : 0;

        public void Reset()
        {
            LineBreakCount = 0;
            IsSeparatorRequired = false;
        }
    }
}