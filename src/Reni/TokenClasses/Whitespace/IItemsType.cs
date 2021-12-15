using System.Collections.Generic;
using hw.Scanner;

namespace Reni.TokenClasses.Whitespace
{
    interface IItemsType
    {
        IEnumerable<WhiteSpaceItem> GetItems(SourcePart sourcePart, IParent parent);
    }
}