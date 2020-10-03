using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Helper;

namespace Reni.Helper
{
    abstract class TreeWithParent<TResult, TTarget>
        : DumpableObject
            , ValueCache.IContainer
        where TTarget : ITree<TTarget>
        where TResult : TreeWithParent<TResult, TTarget>
    {
        public readonly TResult Parent;
        public readonly TTarget Target;

        protected TreeWithParent(TTarget target, TResult parent)
        {
            Target = target;
            Parent = parent;
        }

        internal IEnumerable<TResult> Children => Target.ChildrenCount.Select(Child);
        ValueCache ValueCache.IContainer.Cache { get; } = new ValueCache();

        protected abstract TResult Create(TTarget target, TResult parent);

        internal TResult Child(int index)
            => Target.Child(index) == null
                ? null
                : this.CachedFunction(index, index => Create(Target.Child(index), (TResult)this));
    }
}