using hw.DebugFormatter;
using Reni.TokenClasses;

namespace Reni.SyntaxTree
{
    sealed class CleanupSyntax : Syntax
    {
        internal readonly ValueSyntax Value;

        public CleanupSyntax(BinaryTree anchor, ValueSyntax value)
            : base(anchor, frameItems:FrameItemContainer.Create())
            => Value = value;

        protected override int LeftDirectChildCountKernel => 0;
        protected override int DirectChildCountKernel => 1;

        protected override Syntax GetDirectChildKernel(int index) => Value;
    }
}