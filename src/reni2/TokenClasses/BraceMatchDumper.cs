using hw.DebugFormatter;
using Reni.Parser;

namespace Reni.TokenClasses
{
    sealed class BraceMatchDumper : DumpableObject
    {
        readonly BinaryTree _parent;
        readonly int _depth;

        public BraceMatchDumper(BinaryTree parent, int depth)
            : base(null)
        {
            _parent = parent;
            _depth = depth;
        }

        protected override string Dump(bool isRecursion)
            => _depth > 0 ? base.Dump(isRecursion) : "...";

        protected override string GetNodeDump() => _parent.NodeDump;

        internal BraceMatchDumper Left => Dump(_parent.Left);

        internal ITokenClass TokenClass => _parent.TokenClass;

        internal BraceMatchDumper Right => Dump(_parent.Right);

        internal string Token => _parent.Token.Characters.NodeDump;

        BraceMatchDumper Dump(BinaryTree binaryTree)
            => binaryTree == null
                ? null
                : new BraceMatchDumper(binaryTree, _depth - 1);
    }
}