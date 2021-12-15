using hw.DebugFormatter;
using hw.Scanner;

namespace Reni.TokenClasses.Whitespace
{
    public sealed class Edit : DumpableObject
    {
        [DisableDump]
        internal readonly SourcePart Location;

        [EnableDump(Order = 2)]
        internal readonly string NewText;

        internal readonly string Flag;

        internal Edit(SourcePart location, string newText, string flag)
        {
            Location = location;
            NewText = newText;
            Flag = flag;
        }

        protected override string GetNodeDump() => Flag ?? base.GetNodeDump();

        [EnableDump(Order = 1)]
        string Position => Location.NodeDump;
    }
}