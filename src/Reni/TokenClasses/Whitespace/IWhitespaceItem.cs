using hw.Scanner;

namespace Reni.TokenClasses.Whitespace
{
    interface IWhitespaceItem : IParent
    {
        SourcePart SourcePart { get; }
        IItemType Type { get; }
    }
}