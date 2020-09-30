namespace Reni.Type
{
    interface IRepeaterType
    {
        TypeBase ElementType { get; }
        TypeBase IndexType { get; }
        bool IsMutable { get; }
    }
}