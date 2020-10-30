using hw.DebugFormatter;
using hw.Scanner;

namespace ReniUI.Formatting
{
    public sealed class Edit : DumpableObject
    {
        public static Edit Create(string flag, SourcePart location, string newText = "") 
            => new Edit(location, newText, flag);
        public readonly SourcePart Location;
        public readonly string NewText;
        public readonly string Flag;

        Edit(SourcePart location, string newText, string flag)
        {
            Location = location;
            NewText = newText;
            Flag = flag;
        }

        protected override string GetNodeDump() => Flag ?? base.GetNodeDump();
    }
}