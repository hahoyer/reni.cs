using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Scanner;

namespace ReniUI.Formatting
{
    interface IResultItem
    {
        [DisableDump]
        SourcePart SourcePart {get;}

        [DisableDump]
        string VisibleText {get;}

        [DisableDump]
        string NewText {get;}

        [DisableDump]
        int RemoveCount {get;}

        IEnumerable<IResultItem> Combine(IResultItem right);
        IEnumerable<IResultItem> CombineBack(WhiteSpace left);
        IEnumerable<IResultItem> CombineBack(NewWhiteSpace left);
    }
}