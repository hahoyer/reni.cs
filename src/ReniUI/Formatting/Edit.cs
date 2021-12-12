using hw.DebugFormatter;
using hw.Scanner;

namespace ReniUI.Formatting
{
    public sealed class Edit : DumpableObject, ISourcePartEdit
    {
        [DisableDump]
        internal readonly SourcePart Location;

        [EnableDump(Order = 1)]
        string Position => Location.NodeDump;

        [EnableDump(Order = 2)]
        internal readonly string NewText;

        readonly string Flag;

        Edit(SourcePart location, string newText, string flag)
        {
            Location = location;
            NewText = newText;
            Flag = flag;
            StopByObjectIds(230);
        }

        bool ISourcePartEdit.HasLines => NewText.Contains("\n");
        ISourcePartEdit ISourcePartEdit.Indent(int count) => this.CreateIndent(count);

        SourcePart ISourcePartEdit.SourcePart => Location;

        protected override string GetNodeDump() => Flag ?? base.GetNodeDump();

        public static Edit Create(string flag, SourcePart location, string newText = "")
            => new(location, newText, flag);
    }
}