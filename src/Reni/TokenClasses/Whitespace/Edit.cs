using hw.DebugFormatter;
using hw.Scanner;

namespace Reni.TokenClasses.Whitespace
{
    public sealed class Edit : DumpableObject
    {
        [DisableDump]
        internal readonly SourcePart Remove;

        [EnableDump(Order = 2)]
        internal readonly string Insert;

        internal readonly string Flag;

        internal Edit(SourcePart remove, string insert, string flag)
        {
            Remove = remove;
            Insert = insert;
            Flag = flag;
        }

        protected override string GetNodeDump() => Flag ?? base.GetNodeDump();

        [EnableDump(Order = 1)]
        string Position => Remove.NodeDump;
    }
}