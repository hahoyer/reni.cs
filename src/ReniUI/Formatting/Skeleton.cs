using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Scanner;

namespace ReniUI.Formatting
{
    sealed class Skeleton : DumpableObject, IResultItem
    {
        readonly SourcePart Token;

        internal Skeleton(SourcePart token) => Token = token;

        SourcePart IResultItem.SourcePart => Token;
        string IResultItem.VisibleText => Token.Id;
        string IResultItem.NewText => "";
        int IResultItem.RemoveCount => 0;

        IEnumerable<IResultItem> IResultItem.Combine(IResultItem right) => null;
        IEnumerable<IResultItem> IResultItem.CombineBack(WhiteSpace left) => null;
        IEnumerable<IResultItem> IResultItem.CombineBack(NewWhiteSpace left) => null;

        protected override string GetNodeDump() => Token.Id;
    }
}