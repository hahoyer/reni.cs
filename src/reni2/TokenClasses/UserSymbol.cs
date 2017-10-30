using hw.DebugFormatter;

namespace Reni.TokenClasses
{
    sealed class UserSymbol : Definable
    {
        internal UserSymbol(string name) => Id = name;

        [EnableDump]
        public override string Id { get; }

        protected override string GetNodeDump() { return base.GetNodeDump() + "."+ Id; }
    }
}