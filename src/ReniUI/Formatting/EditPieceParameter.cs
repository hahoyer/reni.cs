using hw.DebugFormatter;

namespace ReniUI.Formatting
{
    sealed class EditPieceParameter : DumpableObject
    {
        public int Indent;

        public bool IsEndOfFile;
        public bool IsSeparatorRequired;
        public int LineBreakCount;
        readonly Configuration Configuration;

        public EditPieceParameter(Configuration configuration) => Configuration = configuration;

        public int IndentCharacterCount => Indent > 0? Indent * Configuration.IndentCount : 0;

        public void Reset()
        {
            LineBreakCount = 0;
            IsSeparatorRequired = false;
        }

        protected override string GetNodeDump()
        {
            var result 
                = $"{(Indent > 0? Indent + ">>" : "")}" +
                           $"{(Indent < 0? "<<" + Indent : "")}" +
                           $"{(IsSeparatorRequired? "sep!" : "")}" +
                           $"{(LineBreakCount > 0? "l" + LineBreakCount : "")}" +
                           $"{(IsEndOfFile? "<eof>" : "")}";
            return result == ""? base.GetNodeDump() : result;
        }
    }
}