namespace Reni.Helper;

abstract class TreeWithParent<TResult, TTarget> : DumpableObject, ValueCache.IContainer
    where TTarget : ITree<TTarget>
    where TResult : TreeWithParent<TResult, TTarget>
{
    [DisableDump]
    internal readonly TTarget FlatItem;

    [DisableDump]
    internal readonly TResult? Parent;

    internal TResult?[] DirectChildren
    {
        get
        {
            var cachedItem = this.CachedItem(() => FlatItem.DirectChildCount.Select(CreateDirectChild).ToArray());
            return
                //IsInDump && !cachedItem.IsValid? null : 
                cachedItem.Value;
        }
    }

    protected TreeWithParent(TTarget flatItem, TResult? parent)
    {
        FlatItem = flatItem;
        Parent = parent;
    }

    ValueCache ValueCache.IContainer.Cache { get; } = new();

    protected abstract TResult? Create(TTarget flatItem);

    TResult? CreateDirectChild(int index)
    {
        var node = FlatItem.GetDirectChild(index);
        return node == null? null : Create(node);
    }

    protected TResult? GetDirectChild(int index)
        => index < 0 || index >= FlatItem.DirectChildCount? null : DirectChildren[index];
}