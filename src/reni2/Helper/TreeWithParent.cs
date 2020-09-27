using hw.DebugFormatter;
using hw.Helper;

namespace Reni.Helper
{
    abstract class TreeWithParent<TResult, TTarget> : DumpableObject, ValueCache.IContainer
        where TTarget : ITree<TTarget>
        where TResult : TreeWithParent<TResult, TTarget> 
    {
        ValueCache ValueCache.IContainer.Cache {get;} = new ValueCache();
        public readonly TResult Parent;
        public readonly TTarget Target;

        protected TreeWithParent(TTarget target, TResult parent)
        {
            Target = target;
            Parent = parent;
        }

        protected abstract TResult Create(TTarget target, TResult parent);

        internal TResult Left
            => Target.Left == null ? null : this.CachedValue(() => Create(Target.Left, (TResult)this));

        internal TResult Right
            => Target.Right == null ? null : this.CachedValue(() => Create(Target.Right, (TResult)this));


    }

    abstract class TreeWithParentExtended<TResult, TTarget> : TreeWithParent<TResult, TTarget>, ITree<TResult>
        where TResult : TreeWithParentExtended<TResult, TTarget> 
        where TTarget : class, ITree<TTarget>
    {
        protected TreeWithParentExtended(TTarget target, TResult parent)
            : base(target, parent) {}

        TResult ITree<TResult>.Left => Left;
        TResult ITree<TResult>.Right => Right;

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