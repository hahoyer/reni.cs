using hw.DebugFormatter;
using hw.Scanner;

namespace ReniUI.Formatting
{
    public sealed class Edit : DumpableObject, ISourcePartEdit
    {
        public readonly SourcePart Location;
        public readonly string NewText;
        public readonly string Flag;

        Edit(SourcePart location, string newText, string flag)
        {
            Location = location;
            NewText = newText;
            Flag = flag;
        }

        bool ISourcePartEdit.HasLines => NewText.Contains("\n");

        SourcePart ISourcePartEdit.SourcePart => Location;
        ISourcePartEdit ISourcePartEdit.Indent(int count) => this.CreateIndent(count);

        protected override string GetNodeDump() => Flag ?? base.GetNodeDump();

        public static Edit Create(string flag, SourcePart location, string newText = "")
            => new Edit(location, newText, flag);
    }
}