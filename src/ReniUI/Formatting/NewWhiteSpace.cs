using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;

namespace ReniUI.Formatting
{
    sealed class NewWhiteSpace : DumpableObject, IResultItem
    {
        internal readonly int Count;
        internal readonly bool IsLineBreak;

        internal NewWhiteSpace(int count, bool isLineBreak)
        {
            Count = count;
            IsLineBreak = isLineBreak;
            (Count > 0).Assert();
        }

        SourcePart IResultItem.SourcePart => null;
        string IResultItem.VisibleText => Text;
        string IResultItem.NewText => Text;
        int IResultItem.RemoveCount => 0;

        IEnumerable<IResultItem> IResultItem.Combine(IResultItem right) => right.CombineBack(this);

        IEnumerable<IResultItem> IResultItem.CombineBack(WhiteSpace left)
            => IsLineBreak == left.IsLineBreak
                ? left.IsVisible
                    ? new IResultItem[] {this, left}
                    : left.Precede(Count - 1)
                : null;

        IEnumerable<IResultItem> IResultItem.CombineBack(NewWhiteSpace left)
        {
            if(IsLineBreak != left.IsLineBreak)
                return null;

            NotImplementedMethod(left);
            return null;
        }

        string Text => (IsLineBreak ? "\n" : " ").Repeat(Count);

        protected override string GetNodeDump() => "new(" + (IsLineBreak ? "\\n" : "_") + ")";
    }
}