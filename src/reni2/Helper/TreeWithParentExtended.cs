using System.Linq;
using hw.DebugFormatter;
using hw.Helper;

namespace Reni.Helper
{
    abstract class TreeWithParentExtended<TResult, TTarget> : TreeWithParent<TResult, TTarget>, ITree<TResult>
        where TResult : TreeWithParentExtended<TResult, TTarget>
        where TTarget : class, ITree<TTarget>
    {
        protected TreeWithParentExtended(TTarget target, TResult parent)
            : base(target, parent) { }

        int DirectChildCount => Target.DirectChildCount;
        int LeftDirectChildCount => Target.LeftDirectChildCount;

        [DisableDump]
        TResult[] LeftSiblings => this.CachedValue(() => LeftDirectChildCount.Select(GetDirectChild).ToArray());

        [DisableDump]
        TResult[] RightSiblings => this.CachedValue(GetRightSiblings);

        [DisableDump]
        internal TResult LeftMost => this.GetNodesFromLeftToRight().First();

        [DisableDump]
        internal TResult RightMost => this.GetNodesFromRightToLeft().First();

        [DisableDump]
        internal TResult LeftNeighbor => RightMostLeftSibling?.RightMost ?? LeftParent;

        [DisableDump]
        internal TResult RightNeighbor => LeftMostRightSibling?.LeftMost ?? RightParent;

        [DisableDump]
        internal TResult RightMostLeftSibling => GetDirectChild(LeftDirectChildCount - 1);

        [DisableDump]
        internal TResult LeftMostRightSibling => GetDirectChild(LeftDirectChildCount);

        [DisableDump]
        internal bool IsLeftChild => Parent?.RightMostLeftSibling == this;

        [DisableDump]
        internal bool IsRightChild => Parent?.LeftMostRightSibling == this;

        [DisableDump]
        TResult LeftParent
            => Parent != null && Parent.LeftSiblings.Contains(this)
                ? Parent.LeftParent
                : Parent;

        [DisableDump]
        TResult RightParent
            => Parent != null && Parent.RightSiblings.Contains(this)
                ? Parent.RightParent
                : Parent;

        [DisableDump]
        int ITree<TResult>.DirectChildCount => DirectChildCount;

        TResult ITree<TResult>.GetDirectChild(int index) => GetDirectChild(index);

        [DisableDump]
        int ITree<TResult>.LeftDirectChildCount => Target.LeftDirectChildCount;

        TResult[] GetRightSiblings()
            => (DirectChildCount - LeftDirectChildCount)
                .Select(index => GetDirectChild(index + LeftDirectChildCount))
                .ToArray();
    }
}