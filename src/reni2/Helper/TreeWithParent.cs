using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;

namespace Reni.Helper
{
    abstract class TreeWithParent<TResult, TTarget> : DumpableObject, ValueCache.IContainer
        where TTarget : ITree<TTarget>
        where TResult : TreeWithParent<TResult, TTarget>
    {
        internal readonly TResult Parent;
        internal readonly TTarget Target;

        protected TreeWithParent(TTarget target, TResult parent)
        {
            Target = target;
            Parent = parent;
        }

        internal IEnumerable<TResult> DirectChildren
            => Target.DirectChildCount.Select(GetDirectChild).Where(child => child != null);

        TResult Center => this as TResult;

        ValueCache ValueCache.IContainer.Cache { get; } = new ValueCache();

        protected abstract TResult Create(TTarget target, TResult parent);

        protected TResult GetDirectChild(int index)
        {
            var node = Target.GetDirectChild(index);
            return node == null? null :
                this.CachedFunction(index, index => Create(node, Center));
        }
    }
}