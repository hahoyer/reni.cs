using Reni.Basics;
using Reni.Context;
using Reni.TokenClasses;

namespace Reni.SyntaxTree
{
    sealed class EmptyList : ValueSyntax.NoChildren
    {
        public EmptyList(BinaryTree anchor)
            : base(null,anchor, null, null)
        {
            StopByObjectIds();
            AssertValid();
        }

        public EmptyList(BinaryTree anchorLeft, BinaryTree anchorRight, FrameItemContainer frameItems)
            : base(null, frameItems.LeftMostRightItem, null, frameItems.WithoutLeftMostRightItem)
        {
            StopByObjectIds();
            AssertValid();
        }

        protected override string GetNodeDump() => "()";

        internal override Result ResultForCache(ContextBase context, Category category)
            => context.RootContext.VoidType.Result(category);
    }
}