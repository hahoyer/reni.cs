using hw.DebugFormatter;

namespace Reni.Helper
{
    abstract class BinaryTreeWithParentExtended<TResult, TTarget> : BinaryTreeWithParent<TResult, TTarget>, IBinaryTree<TResult>
        where TResult : BinaryTreeWithParentExtended<TResult, TTarget> 
        where TTarget : class, IBinaryTree<TTarget>
    {
        protected BinaryTreeWithParentExtended(TTarget target, TResult parent)
            : base(target, parent) {}

        TResult IBinaryTree<TResult>.Left => Left;
        TResult IBinaryTree<TResult>.Right => Right;

        [DisableDump]
        internal TResult LeftMost => Left?.LeftMost ?? (TResult) this;

        [DisableDump]
        internal TResult RightMost => Right?.RightMost ?? (TResult) this;

        [DisableDump]
        internal TResult LeftNeighbor => Left?.RightMost ?? LeftParent;

        [DisableDump]
        internal TResult RightNeighbor => Right?.LeftMost ?? RightParent;

        [DisableDump]
        internal bool IsLeftChild => Parent?.Left == this;

        [DisableDump]
        internal bool IsRightChild => Parent?.Right == this;

        [DisableDump]
        TResult LeftParent 
            => Parent != null && Parent.Target.Left == Target 
                ? Parent.LeftParent 
                : Parent;

        [DisableDump]
        TResult RightParent 
            => Parent != null && Parent.Target.Right == Target
                ? Parent.RightParent
                : Parent;
    }
}