using System.Collections.Generic;
using hw.Scanner;

namespace Reni.TokenClasses.Whitespace
{
    interface IItemsType
    {
        IEnumerable<WhitespaceItem> GetItems(SourcePart sourcePart, IParent parent);
    }
}