namespace Reni.Helper
{
    interface ITree<out TTarget>
    {
        TTarget Left {get;}
        TTarget Right{get;}
    }
}