namespace Reni.Helper
{
    interface ITree<out TTarget>
    {
        int LeftDirectChildCount { get; }
        int DirectChildCount { get; }
        TTarget GetDirectChild(int index);
    }
}