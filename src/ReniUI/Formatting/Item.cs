using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using hw.Scanner;
using Reni.Parser;

namespace ReniUI.Formatting
{
    interface ISourceItem
    {
        [DisableDump]
        SourcePart Data { get; }
    }

    interface IResultItem
    {
        [DisableDump]
        string Text { get; }
        IEnumerable<IResultItem> Combine(IResultItem right);
        IEnumerable<IResultItem> CombineBack(SpaceTextItem left);
        IEnumerable<IResultItem> CombineBack(WhiteSpaceTokenItem left);
        IEnumerable<IResultItem> CombineBack(LineBreakTextItem left);
    }

    sealed class SpaceTextItem : DumpableObject, IResultItem
    {
        internal readonly int Count;

        internal SpaceTextItem(int count)
        {
            Count = count;
            Tracer.Assert(Count > 0);
        }

        string IResultItem.Text { get { return " ".Repeat(Count); } }

        IEnumerable<IResultItem> IResultItem.Combine(IResultItem right) => right.CombineBack(this);

        IEnumerable<IResultItem> IResultItem.CombineBack(SpaceTextItem left)
        {
            NotImplementedMethod(left);
            return null;
        }

        IEnumerable<IResultItem> IResultItem.CombineBack(LineBreakTextItem left) => null;

        IEnumerable<IResultItem> IResultItem.CombineBack(WhiteSpaceTokenItem left)
        {
            if(!left.IsSpace)
                return null;

            return left.IsVisible
                ? new IResultItem[] {this, left}
                : Count == 1
                    ? new[] {left.VisibleVersion}
                    : new[] {new SpaceTextItem(Count - 1), left.VisibleVersion};
        }
    }

    sealed class LineBreakTextItem : DumpableObject, IResultItem
    {
        internal readonly int Count;

        public LineBreakTextItem(int count)
        {
            Count = count;
            Tracer.Assert(Count > 0);
        }

        string IResultItem.Text { get { return "\n".Repeat(Count); } }

        IEnumerable<IResultItem> IResultItem.Combine(IResultItem right) => right.CombineBack(this);

        IEnumerable<IResultItem> IResultItem.CombineBack(LineBreakTextItem left)
        {
            NotImplementedMethod(left);
            return null;
        }

        IEnumerable<IResultItem> IResultItem.CombineBack(SpaceTextItem left)
        {
            NotImplementedMethod(left);
            return null;
        }

        IEnumerable<IResultItem> IResultItem.CombineBack(WhiteSpaceTokenItem left)
        {
            NotImplementedMethod(left);
            return null;
        }
    }

    sealed class WhiteSpaceTokenItem : DumpableObject, IResultItem, ISourceItem
    {
        readonly WhiteSpaceToken Token;

        internal readonly bool IsVisible;
        internal bool IsSpace => Token.Characters.Id == " ";
        internal bool IsLineBreak => Token.IsLineBreak();

        public WhiteSpaceTokenItem(WhiteSpaceToken token, bool isVisible)
        {
            Token = token;
            IsVisible = isVisible;
        }

        SourcePart ISourceItem.Data => IsVisible ? Token.Characters : null;
        string IResultItem.Text => IsVisible ? Token.Characters.Id : "";

        IEnumerable<IResultItem> IResultItem.Combine(IResultItem right) => right.CombineBack(this);
        IEnumerable<IResultItem> IResultItem.CombineBack(WhiteSpaceTokenItem left)
        {
            if(IsSpace && left.IsSpace || IsLineBreak && left.IsLineBreak)
            {
                if(!IsVisible && left.IsVisible)
                    return new IResultItem[] {this, left};
            }

            return null;
        }

        IEnumerable<IResultItem> IResultItem.CombineBack(LineBreakTextItem left)
        {
            if (!IsLineBreak|| IsVisible)
                return null;

            return left.Count == 1
                ? new[] { VisibleVersion }
                : new[] { VisibleVersion, new SpaceTextItem(left.Count - 1) };
        }

        IEnumerable<IResultItem> IResultItem.CombineBack(SpaceTextItem left)
        {
            if(!IsSpace || IsVisible)
                return null;

            return left.Count == 1
                ? new[] {VisibleVersion}
                : new[] {VisibleVersion, new LineBreakTextItem(left.Count - 1), };
        }

        protected override string GetNodeDump() { return (IsVisible ? "" : "In") + "Visible " + Token.NodeDump; }

        [DisableDump]
        internal IResultItem VisibleVersion => IsVisible ? this : new WhiteSpaceTokenItem(Token, true);
    }

    sealed class TokenItem : DumpableObject, IResultItem, ISourceItem
    {
        readonly IToken Token;
        public TokenItem(IToken token) { Token = token; }

        SourcePart ISourceItem.Data => Token.Characters;
        string IResultItem.Text => Token.Id;
        IEnumerable<IResultItem> IResultItem.Combine(IResultItem right) => null;
        IEnumerable<IResultItem> IResultItem.CombineBack(SpaceTextItem left) => null;
        IEnumerable<IResultItem> IResultItem.CombineBack(WhiteSpaceTokenItem left) => null;
        IEnumerable<IResultItem> IResultItem.CombineBack(LineBreakTextItem left) => null;
        protected override string GetNodeDump() { return Token.Id; }
    }


    sealed class ResultItems : DumpableObject
    {
        readonly List<IResultItem> Data;

        public ResultItems() { Data = new List<IResultItem>(); }
        public ResultItems(IEnumerable<IResultItem> data) { Data = new List<IResultItem>(data); }

        public bool IsEmpty => Data.All(item => item.Text == "");

        public ResultItems Combine(ResultItems other)
        {
            var result = new ResultItems(Data);
            foreach(var item in other.Data)
                result.Add(item);
            return result;
        }

        internal void Add(WhiteSpaceToken token, bool isVisible)
        {
            Add(new WhiteSpaceTokenItem(token, isVisible));
        }

        internal void Add(IToken token) { Add(new TokenItem(token)); }

        internal void AddLineBreak(int count)
        {
            if (count > 0)
                Add(new LineBreakTextItem(count));
        }

        internal void AddSpaces(int count)
        {
            if(count > 0)
                Add(new SpaceTextItem(count));
        }

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
            var trace = false && HasOnlySpaces(left) && HasOnlySpaces(right);
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

        static bool HasOnlySpaces(IResultItem left)
        {
            return left != null && left.Text != "" && left.Text.All(c => c == ' ');
        }

        internal string Format(SourcePart targetPart = null)
        {
            if(targetPart == null)
                return Text;

            var result = "";
            IEnumerable<IResultItem> data = Data;

            var start = Data.IndexWhere(item => (item as ISourceItem)?.Data?.Intersect(targetPart) != null);
            if(start != null)
                data = Data.Skip(start.Value);

            var f = (ISourceItem) data.FirstOrDefault();
            if(f == null)
                return result;

            var start1 = targetPart.Position - f.Data.Position;
            foreach(var item in data)
            {
                var s = item as ISourceItem;
                if(s == null)
                {
                    Tracer.Assert(start1 == 0);
                    result += item.Text;
                }

                else if(s.Data != null)
                {
                    if(targetPart.EndPosition < s.Data.Position)
                        return result;

                    var length = Math.Min(s.Data.EndPosition, targetPart.EndPosition)
                                 - start1
                                 - s.Data.Position;

                    result += item.Text.Substring(start1, length);

                    if(targetPart.EndPosition == s.Data.EndPosition)
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

            var start = Data.IndexWhere(item => (item as ISourceItem)?.Data.Intersect(targetPart) != null);
            if(start != null)
                data = Data.Skip(start.Value);

            var f = (ISourceItem) data.FirstOrDefault();
            if(f == null)
                yield break;

            var newText = "";
            var removeCount = 0;

            foreach(var item in data)
            {
                var s = item as ISourceItem;
                if(s == null)
                    newText += item.Text;
                else
                {
                    if(newText != "" || removeCount > 0)
                    {
                        yield return new EditPiece
                        {
                            NewText = newText,
                            RemoveCount = removeCount,
                            Position = s.Data.Position
                        };
                        newText = "";
                        removeCount = 0;
                    }

                    if(targetPart.EndPosition <= s.Data.Position)
                        yield break;
                }
            }
        }

        string Text { get { return Data.Aggregate("", (c, n) => c + n.Text); } }

        protected override string GetNodeDump() => Data.Count + "->" + Text + "<-";

        internal bool HasInnerLineBreaks() { return Data.Skip(1).Any(item => item.Text.Contains("\n")); }

        public override string ToString() => Text;
    }

    public sealed class EditPiece : DumpableObject
    {
        internal int Position;
        internal int RemoveCount;
        internal string NewText;
    }
}