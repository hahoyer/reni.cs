using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Scanner;
using Reni.Parser;

namespace ReniUI.Formatting {
    sealed class WhiteSpace : DumpableObject, IResultItem
    {
        public static WhiteSpace Create(IItem token, bool isVisible)
        {
            var isLineBreak = token.IsLineBreak();
            var sourcePart = token.SourcePart;

            if(isLineBreak || sourcePart.Id != "\t")
                return new WhiteSpace(sourcePart, isLineBreak, isVisible);

            NotImplementedFunction(token, isLineBreak);
            return null;
        }


        readonly SourcePart Token;
        internal readonly bool IsLineBreak;
        internal readonly bool IsVisible;

        WhiteSpace(SourcePart token, bool isLineBreak, bool isVisible)
        {
            Token = token;
            IsLineBreak = isLineBreak;
            IsVisible = isVisible;
        }

        SourcePart IResultItem.SourcePart => IsVisible ? Token : null;
        string IResultItem.VisibleText => IsVisible ? Token.Id : "";
        string IResultItem.NewText => "";
        int IResultItem.RemoveCount => IsVisible ? 0 : Token.Length;

        IEnumerable<IResultItem> IResultItem.Combine(IResultItem right) => right.CombineBack(this);

        IEnumerable<IResultItem> IResultItem.CombineBack(WhiteSpace left)
            => IsLineBreak == left.IsLineBreak && !IsVisible && left.IsVisible
                ? new[] {left.MakeInvisible, MakeVisible}
                : null;

        IEnumerable<IResultItem> IResultItem.CombineBack(NewWhiteSpace left)
            => IsLineBreak == left.IsLineBreak && !IsVisible
                ? Preceed(left.Count - 1)
                : null;

        internal IEnumerable<IResultItem> Preceed(int count)
            => count > 0
                ? new[] {new NewWhiteSpace(count, IsLineBreak), MakeVisible}
                : new[] {MakeVisible};

        [DisableDump]
        IResultItem MakeInvisible => Create(false);
        [DisableDump]
        internal IResultItem MakeVisible => Create(true);

        WhiteSpace Create(bool isVisible) => new WhiteSpace(Token, IsLineBreak, isVisible);

        protected override string GetNodeDump()
            => (IsVisible ? "" : "in") + "visible(" + (IsLineBreak ? "\\n" : "_") + ")";
    }
}