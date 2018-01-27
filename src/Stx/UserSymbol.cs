using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Parser;

namespace Stx
{
    sealed class UserSymbol : TokenClass, IAliasKeeper
    {
        [EnableDump]
        readonly string Name;

        [EnableDump]
        readonly IList<string> Names = new List<string>();

        public UserSymbol(string name) => Name = name;

        void IAliasKeeper.Add(string id) => Names.Add(id);
        public override string Id => Name;
    }
}