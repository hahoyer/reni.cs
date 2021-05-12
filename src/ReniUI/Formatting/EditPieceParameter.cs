using hw.DebugFormatter;

namespace ReniUI.Formatting
{
    sealed class EditPieceParameter : DumpableObject
    {
        internal int Indent;
        internal readonly Configuration Configuration;
        public EditPieceParameter(Configuration configuration) => Configuration = configuration;

        public int IndentCharacterCount => Indent > 0? Indent * Configuration.IndentCount : 0;

        protected override string GetNodeDump()
        {
            var result 
                = $"{(Indent > 0? Indent + ">>" : "")}" +
                           $"{(Indent < 0? "<<" + Indent : "")}";
            return result == ""? base.GetNodeDump() : result;
        }
    }
}