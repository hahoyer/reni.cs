namespace Reni.Helper
{
    interface IBinaryTree<out TTarget>
    {
        TTarget Left {get;}
        TTarget Right{get;}
    }

    interface ITree<out TTarget>
    {
        int ChildrenCount { get; }
        TTarget Child(int index);
    }

}