namespace Reni.Helper;

abstract class TreeWithParentExtended<TResult, TTarget> : TreeWithParent<TResult, TTarget>, ITree<TResult>
    where TResult : TreeWithParentExtended<TResult, TTarget>
    where TTarget : class, ITree<TTarget>
{
    int DirectChildCount => FlatItem.DirectChildCount;
    int LeftDirectChildCount => FlatItem.LeftDirectChildCount;

    [DisableDump]
    TResult[] LeftChildren => this.CachedValue(() => LeftDirectChildCount.Select(GetDirectChild).ToArray());

    [DisableDump]
    TResult[] RightChildren => this.CachedValue(GetRightChildren);

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
        => Parent != null && Parent.LeftChildren.Contains(this)
            ? Parent.LeftParent
            : Parent;

    [DisableDump]
    TResult RightParent
        => Parent != null && Parent.RightChildren.Contains(this)
            ? Parent.RightParent
            : Parent;

    protected TreeWithParentExtended(TTarget flatItem, TResult parent)
        : base(flatItem, parent) { }

    [DisableDump]
    int ITree<TResult>.DirectChildCount => DirectChildCount;

    TResult ITree<TResult>.GetDirectChild(int index) => GetDirectChild(index);

    [DisableDump]
    int ITree<TResult>.LeftDirectChildCount => FlatItem.LeftDirectChildCount;

    TResult[] GetRightChildren()
        => (DirectChildCount - LeftDirectChildCount)
            .Select(index => GetDirectChild(index + LeftDirectChildCount))
            .ToArray();
}