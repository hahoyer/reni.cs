using hw.DebugFormatter;
using Reni.Parser;
using Reni.TokenClasses;

namespace Reni.SyntaxTree
{
    sealed class CleanupSyntax : Syntax
    {
        internal readonly ValueSyntax Value;

        public CleanupSyntax(BinaryTree anchor, ValueSyntax value)
            : base(anchor)
        {
            Value = value ?? new EmptyList(null);
            AssertValid();
        }

        internal override int LeftDirectChildCount => 0;
        protected override int DirectChildCount => 1;

        void AssertValid()
        {
            (Anchor != null).Assert();
            (Value != null).Assert();
        }

        protected override Syntax GetDirectChild(int index) => Value;
    }
}