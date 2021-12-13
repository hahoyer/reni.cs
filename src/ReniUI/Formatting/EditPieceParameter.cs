using hw.DebugFormatter;

namespace ReniUI.Formatting
{
    sealed class EditPieceParameter : DumpableObject, IEditPiecesConfiguration
    {
        readonly Configuration Configuration;
        int Indent;
        public EditPieceParameter(Configuration configuration) => Configuration = configuration;

        int IEditPiecesConfiguration.Indent
        {
            get => Indent;
            set => Indent = value;
        }

        protected override string GetNodeDump()
        {
            var result
                = $"{(Indent > 0? Indent + ">>" : "")}" +
                $"{(Indent < 0? "<<" + Indent : "")}";
            return result == ""? base.GetNodeDump() : result;
        }

        public int IndentCharacterCount => Indent > 0? Indent * Configuration.IndentCount : 0;
    }
}