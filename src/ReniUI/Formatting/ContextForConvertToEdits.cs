using hw.DebugFormatter;

namespace ReniUI.Formatting
{
    sealed class ContextForConvertToEdits : DumpableObject
    {
        internal readonly Configuration Configuration;

        public int Indent;
        public bool IsEndOfFile;
        public bool IsSeparatorRequired;
        public int LineBreakCount;

        public ContextForConvertToEdits(Configuration configuration) => Configuration = configuration;

        internal void Reset()
        {
            LineBreakCount = 0;
            IsSeparatorRequired = false;
        }
    }
}