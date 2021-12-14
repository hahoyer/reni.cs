using hw.DebugFormatter;
using hw.Scanner;

namespace Reni.TokenClasses.Whitespace
{
    sealed class ItemPrototype : DumpableObject
    {
        internal readonly IItemType Type;
        internal readonly IMatch Match;

        public ItemPrototype(IItemType type, IMatch match)
        {
            Type = type;
            Match = match;
        }
    }
}