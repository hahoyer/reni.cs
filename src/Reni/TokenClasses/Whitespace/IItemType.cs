namespace Reni.TokenClasses.Whitespace
{
    interface IItemType
    {
        [DisableDump]
        bool IsSeparatorRequired { get; }
    }
}