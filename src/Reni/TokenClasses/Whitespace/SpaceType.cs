namespace Reni.TokenClasses.Whitespace;

sealed class SpaceType : DumpableObject, ISpace
{
    internal static readonly SpaceType Instance = new();
    bool IItemType.IsSeparatorRequired => false;
}