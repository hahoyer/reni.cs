using hw.DebugFormatter;
using hw.Scanner;

namespace ReniUI.Formatting
{
    public sealed class Edit : DumpableObject
    {
        public static Edit Create(SourcePart location, string newText = "") {return new Edit(location, newText);}
        public readonly SourcePart Location;
        public readonly string NewText;

        Edit(SourcePart location, string newText = "")
        {
            Location = location;
            NewText = newText;
            StopByObjectIds(689);
        }
    }
}