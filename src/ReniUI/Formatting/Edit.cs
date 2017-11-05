using hw.DebugFormatter;
using hw.Scanner;

namespace ReniUI.Formatting
{
    public class Edit : DumpableObject
    {
        public SourcePart Location;
        public string NewText;
    }
}