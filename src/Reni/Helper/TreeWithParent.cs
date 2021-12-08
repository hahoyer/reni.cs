using System.Linq;
using hw.DebugFormatter;
using hw.Helper;

namespace Reni.Helper
{
    abstract class TreeWithParent<TResult, TTarget> : DumpableObject, ValueCache.IContainer
        where TTarget : ITree<TTarget>
        where TResult : TreeWithParent<TResult, TTarget>
    {
        [DisableDump]
        internal readonly TTarget FlatItem;

        [DisableDump]
        internal readonly TResult Parent;

        protected TreeWithParent(TTarget flatItem, TResult parent)
        {
            FlatItem = flatItem;
            Parent = parent;
        }

        internal TResult[] DirectChildren
            => this.CachedValue(() => FlatItem.DirectChildCount.Select(CreateDirectChild).ToArray());

        ValueCache ValueCache.IContainer.Cache { get; } = new ValueCache();

        TResult CreateDirectChild(int index)
        {
            var node = FlatItem.GetDirectChild(index);
            return node == null? null : Create(node);
        }

        protected abstract TResult Create(TTarget flatItem);

        protected TResult GetDirectChild(int index)
            => index < 0 || index >= FlatItem.DirectChildCount? null : DirectChildren[index];
    }
}