using System.Collections.Generic;
using hw.Scanner;

namespace Reni.TokenClasses {
    public interface IComparator
    {
        IEqualityComparer<IItem> WhiteSpaceComparer {get;}
    }
}