using hw.DebugFormatter;
using Reni.TokenClasses;

namespace Reni.SyntaxTree
{
    sealed class CleanupSyntax : Syntax
    {
        internal readonly ValueSyntax Value;

        public CleanupSyntax(BinaryTree anchor, ValueSyntax value)
            : base(anchor)
            => Value = value;

        internal override int LeftDirectChildCount => 0;
        protected override int DirectChildCount => 1;

        protected override Syntax GetDirectChild(int index) => Value;
    }
}