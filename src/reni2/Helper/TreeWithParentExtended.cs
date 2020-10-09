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

        int DirectNodeCount => Target.DirectNodeCount;
        int CenterIndex => this.CachedValue(GetCenterIndex);
        TResult[] LeftSiblings => this.CachedValue(() => CenterIndex.Select(GetDirectNode).ToArray());
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
        internal TResult RightMostLeftSibling => GetDirectNode(CenterIndex - 1);

        [DisableDump]
        internal TResult LeftMostRightSibling => GetDirectNode(CenterIndex + 1);

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

        int ITree<TResult>.DirectNodeCount => DirectNodeCount;
        TResult ITree<TResult>.GetDirectNode(int index) => GetDirectNode(index);

        TResult[] GetRightSiblings()
            => (DirectNodeCount - CenterIndex - 1)
                .Select(index => GetDirectNode(index + CenterIndex + 1))
                .ToArray();

        int GetCenterIndex() => DirectNodeCount
            .Select()
            .First(index => ReferenceEquals(GetDirectNode(index), this));
    }
}