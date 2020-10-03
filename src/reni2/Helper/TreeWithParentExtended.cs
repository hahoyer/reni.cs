namespace Reni.Helper
{
    abstract class TreeWithParentExtended<TResult, TTarget>
        : TreeWithParent<TResult, TTarget>
            , ITree<TResult>
        where TResult : TreeWithParentExtended<TResult, TTarget>
        where TTarget : class, ITree<TTarget>
    {
        protected TreeWithParentExtended(TTarget target, TResult parent)
            : base(target, parent) { }

        TResult ITree<TResult>.Child(int index) => Child(index);
        int ITree<TResult>.ChildrenCount => Target.ChildrenCount;
    }
}