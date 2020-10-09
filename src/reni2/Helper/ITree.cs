namespace Reni.Helper
{
    interface ITree<out TTarget>
    {
        int DirectNodeCount { get; }
        TTarget GetDirectNode(int index);
    }
}