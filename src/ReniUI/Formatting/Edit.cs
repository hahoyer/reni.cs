using hw.Scanner;

namespace ReniUI.Formatting
{
    public sealed class Edit : DumpableObject, ISourcePartEdit
    {
        [DisableDump]
        internal readonly SourcePart Remove;

        [EnableDump(Order = 1)]
        string Position => Remove.NodeDump;

        [EnableDump(Order = 2)]
        internal readonly string Insert;

        readonly string Flag;

        internal Edit(SourcePart remove, string insert, string flag)
        {
            Remove = remove;
            Insert = insert;
            Flag = flag;
            StopByObjectIds(440);
        }

        bool ISourcePartEdit.IsIndentTarget => Insert.Contains("\n");
        ISourcePartEdit ISourcePartEdit.Indent(int count) => this.CreateIndent(count);

        SourcePart ISourcePartEdit.SourcePart => Remove;

        protected override string GetNodeDump() => Flag ?? base.GetNodeDump();

        public static Edit Create(string flag, SourcePart location, string newText = "")
            => new(location, newText, flag);
    }
}