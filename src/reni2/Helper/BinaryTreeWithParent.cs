using hw.DebugFormatter;
using hw.Helper;

namespace Reni.Helper
{
    abstract class BinaryTreeWithParent<TResult, TTarget> : DumpableObject, ValueCache.IContainer
        where TTarget : IBinaryTree<TTarget>
        where TResult : BinaryTreeWithParent<TResult, TTarget> 
    {
        ValueCache ValueCache.IContainer.Cache {get;} = new ValueCache();
        public readonly TResult Parent;
        public readonly TTarget Target;

        protected BinaryTreeWithParent(TTarget target, TResult parent)
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
}