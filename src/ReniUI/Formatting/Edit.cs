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
            StopByObjectIds(238);
        }

        ISourcePartEdit ISourcePartEdit.AddLineBreaks(int count)
        {
            NotImplementedMethod(count);
            return null;
        }

        bool ISourcePartEdit.HasLines => NewText.Contains("\n");
        ISourcePartEdit ISourcePartEdit.Indent(int count) => this.CreateIndent(count);

        SourcePart ISourcePartEdit.SourcePart => Location;

        protected override string GetNodeDump() => Flag ?? base.GetNodeDump();

        public static Edit Create(string flag, SourcePart location, string newText = "")
            => new(location, newText, flag);
    }
}