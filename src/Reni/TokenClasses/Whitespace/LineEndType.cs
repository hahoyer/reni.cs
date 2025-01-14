namespace Reni.TokenClasses.Whitespace;

sealed class LineEndType : DumpableObject, IVolatileLineBreak
{
    internal static readonly LineEndType Instance = new();
    bool IItemType.IsSeparatorRequired => false;
}