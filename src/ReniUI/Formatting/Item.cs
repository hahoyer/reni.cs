using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using Reni.Parser;

namespace ReniUI.Formatting
{
    interface IResultItem
    {
        IEnumerable<IResultItem> Combine(IResultItem right);
        IEnumerable<IResultItem> CombineBack(WhiteSpace left);
        IEnumerable<IResultItem> CombineBack(NewWhiteSpace left);

        [DisableDump]
        SourcePart SourcePart { get; }

        [DisableDump]
        string VisibleText { get; }
        [DisableDump]
        string NewText { get; }
        [DisableDump]
        int RemoveCount { get; }
    }

    sealed class NewWhiteSpace : DumpableObject, IResultItem
    {
        internal readonly int Count;
        internal readonly bool IsLineBreak;

        internal NewWhiteSpace(int count, bool isLineBreak)
        {
            Count = count;
            IsLineBreak = isLineBreak;
            Tracer.Assert(Count > 0);
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
                    : left.Preceed(Count - 1)
                : null;

        IEnumerable<IResultItem> IResultItem.CombineBack(NewWhiteSpace left)
        {
            if(IsLineBreak != left.IsLineBreak)
                return null;

            NotImplementedMethod(left);
            return null;
        }

        string Text => (IsLineBreak ? "\n" : " ").Repeat(Count);

        protected override string GetNodeDump() => "new("+(IsLineBreak ? "\\n" : "_") + ")";
    }

    sealed class WhiteSpace : DumpableObject, IResultItem
    {
        readonly SourcePart Token;
        internal readonly bool IsLineBreak;
        internal readonly bool IsVisible;

        internal WhiteSpace(SourcePart token, bool isLineBreak, bool isVisible)
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
        IResultItem MakeInvisible => new WhiteSpace(Token, IsLineBreak, false);
        [DisableDump]
        internal IResultItem MakeVisible => new WhiteSpace(Token, IsLineBreak, true);

        protected override string GetNodeDump() => (IsVisible ? "" : "in")+"visible("+(IsLineBreak?"\\n":"_")+")";
    }

    sealed class Skeleton : DumpableObject, IResultItem
    {
        readonly SourcePart Token;

        internal Skeleton(SourcePart token) { Token = token; }

        SourcePart IResultItem.SourcePart => Token;
        string IResultItem.VisibleText => Token.Id;
        string IResultItem.NewText => "";
        int IResultItem.RemoveCount => 0;

        IEnumerable<IResultItem> IResultItem.Combine(IResultItem right) => null;
        IEnumerable<IResultItem> IResultItem.CombineBack(WhiteSpace left) => null;
        IEnumerable<IResultItem> IResultItem.CombineBack(NewWhiteSpace left) => null;

        protected override string GetNodeDump() => Token.Id;
    }


    sealed class ResultItems : DumpableObject
    {
        readonly List<IResultItem> Data;

        public ResultItems() { Data = new List<IResultItem>(); }
        public ResultItems(IEnumerable<IResultItem> data) { Data = new List<IResultItem>(data); }

        public bool IsEmpty => Data.All(item => item.VisibleText == "");

        public ResultItems Combine(ResultItems other)
        {
            var result = new ResultItems(Data);
            foreach(var item in other.Data)
                result.Add(item);
            return result;
        }

        internal void AddLineBreak(int count)
        {
            if(count > 0)
                Add(new NewWhiteSpace(count, true));
        }

        internal void AddSpaces(int count)
        {
            if(count > 0)
                Add(new NewWhiteSpace(count, false));
        }

        internal void Add(WhiteSpaceToken token)
            => Add
                (
                    token.IsComment()
                        ? (IResultItem) new Skeleton(token.Characters)
                        : new WhiteSpace(token.Characters, token.IsLineBreak(), true)
                );

        internal void AddHidden(WhiteSpaceToken token)
        {
            Tracer.Assert(token.IsNonComment());
            Add(new WhiteSpace(token.Characters, token.IsLineBreak(), false));
        }

        internal void Add(IToken token) => Add(new Skeleton(token.Characters));

        void Add(IResultItem item)
        {
            var last = Data.LastOrDefault();
            var newItem = GetNewItems(last, item);

            if(newItem == null)
            {
                Data.Add(item);
                return;
            }

            Data.Remove(last);
            foreach(var n in newItem)
                Add(n);
        }

        IEnumerable<IResultItem> GetNewItems(IResultItem left, IResultItem right)
        {
            var trace = false;
            StartMethodDump(trace, left, right);
            try
            {
                var result = left?.Combine(right)?.ToArray();
                return ReturnMethodDump(result);
            }
            finally
            {
                EndMethodDump();
            }
        }

        internal string Format(SourcePart targetPart = null)
        {
            if(targetPart == null)
                return Text;

            var result = "";
            IEnumerable<IResultItem> data = Data;

            var start = Data.IndexWhere
                (item => item.SourcePart?.Intersect(targetPart) != null);
            if(start != null)
                data = Data.Skip(start.Value);

            if(!data.Any())
                return result;

            var start1 = targetPart.Position - data.First().SourcePart.Position;
            foreach(var item in data)
            {
                var s = item.SourcePart;
                if(s == null)
                {
                    Tracer.Assert(start1 == 0);
                    result += item.VisibleText;
                }
                else
                {
                    if(targetPart.EndPosition < s.Position)
                        return result;

                    var length = Math.Min(s.EndPosition, targetPart.EndPosition)
                        - start1
                        - s.Position;

                    result += item.VisibleText.Substring(start1, length);

                    if(targetPart.EndPosition == s.EndPosition)
                        return result;
                }
                start1 = 0;
            }

            return result;
        }

        internal IEnumerable<EditPiece> GetEditPieces(SourcePart targetPart = null)
        {
            if(targetPart == null)
                yield break;

            IEnumerable<IResultItem> data = Data;

            var start = Data.IndexWhere
                (item => item.SourcePart?.Intersect(targetPart) != null);
            if(start != null)
                data = Data.Skip(start.Value);

            if(!data.Any())
                yield break;

            var newText = "";
            var removeCount = 0;

            foreach(var item in data)
            {
                newText += item.NewText;
                removeCount += item.RemoveCount;

                var sourcePart = item.SourcePart;
                if(sourcePart == null)
                    continue;

                if(newText != "" || removeCount > 0)
                {
                    yield return new EditPiece
                    {
                        NewText = newText,
                        RemoveCount = removeCount,
                        Position = sourcePart.Position
                    };
                    newText = "";
                    removeCount = 0;
                }

                if(targetPart.EndPosition <= sourcePart.Position)
                    yield break;
            }
        }

        string Text { get { return Data.Aggregate("", (c, n) => c + n.VisibleText); } }

        protected override string GetNodeDump() => Data.Count + "->" + Text + "<-";

        internal bool HasInnerLineBreaks()
        {
            return Data.Skip(1).Any(item => item.VisibleText.Contains("\n"));
        }

        public override string ToString() => Text;
    }

    public sealed class EditPiece : DumpableObject
    {
        internal int Position;
        internal int RemoveCount;
        internal string NewText;
    }
}