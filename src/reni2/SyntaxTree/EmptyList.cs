using Reni.Basics;
using Reni.Context;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.SyntaxTree
{
    sealed class EmptyList : ValueSyntax.NoChildren
    {
        public EmptyList(BinaryTree anchor, FrameItemContainer frameItems = null, Issue issue = null)
            : base(anchor, frameItems ?? FrameItemContainer.Create(), issue)
        {
            StopByObjectIds();
            AssertValid();
        }

        public EmptyList(FrameItemContainer frameItems)
            : base(frameItems.LeftMostRightItem, frameItems.WithoutLeftMostRightItem)
        {
            StopByObjectIds();
            AssertValid();
        }

        protected override string GetNodeDump() => "()";

        internal override Result ResultForCache(ContextBase context, Category category)
            => context.RootContext.VoidType.Result(category);
    }
}