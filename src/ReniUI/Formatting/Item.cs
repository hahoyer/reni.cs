using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;

namespace ReniUI.Formatting
{
    interface IResultItem
    {
        string Text { get; }
        IEnumerable<IResultItem> Combine(IResultItem right);
        IEnumerable<IResultItem> CombineBack(string left);
        IEnumerable<IResultItem> CombineBack(HiddenSourceItem left);
        bool Overlapps(SourcePart targetPart);
    }

    sealed class TextItem : DumpableObject, IResultItem
    {
        readonly string Data;

        public TextItem(string data)
        {
            Tracer.Assert(data != "");
            Data = data;
        }

        string IResultItem.Text => Data;

        IEnumerable<IResultItem> IResultItem.Combine(IResultItem right) => right.CombineBack(Data);

        IEnumerable<IResultItem> IResultItem.CombineBack(string left)
        {
            yield return new TextItem(left + Data);
        }

        IEnumerable<IResultItem> IResultItem.CombineBack(HiddenSourceItem left)
        {
            if(Data == left.Data.Id)
                return new[] {new SourceItem(left.Data)};

            if(Data.StartsWith(left.Data.Id))
                return new IResultItem[]
                {
                    new SourceItem(left.Data),
                    new TextItem(Data.Substring(left.Data.Length))
                };

            return null;
        }

        bool IResultItem.Overlapps(SourcePart targetPart) => false;
        protected override string GetNodeDump() => Data;
    }

    sealed class SourceItem : DumpableObject, IResultItem
    {
        internal readonly SourcePart Data;
        public SourceItem(SourcePart data) { Data = data; }
        string IResultItem.Text => Data.Id;

        IEnumerable<IResultItem> IResultItem.Combine(IResultItem right) => null;
        IEnumerable<IResultItem> IResultItem.CombineBack(string left) => null;
        IEnumerable<IResultItem> IResultItem.CombineBack(HiddenSourceItem left) => null;
        bool IResultItem.Overlapps(SourcePart targetPart) => Data.Intersect(targetPart) != null;
        protected override string GetNodeDump() => Data.Id;
    }

    sealed class HiddenSourceItem : DumpableObject, IResultItem
    {
        internal readonly SourcePart Data;
        public HiddenSourceItem(SourcePart data) { Data = data; }
        string IResultItem.Text => "";

        IEnumerable<IResultItem> IResultItem.Combine(IResultItem right) => right.CombineBack(this);
        IEnumerable<IResultItem> IResultItem.CombineBack(HiddenSourceItem left) => null;

        IEnumerable<IResultItem> IResultItem.CombineBack(string left)
        {
            if(left == Data.Id)
                return new[] {new SourceItem(Data)};


            if(left.EndsWith(Data.Id))
                return new IResultItem[]
                {
                    new TextItem(left.Substring(0, left.Length - Data.Length)),
                    new SourceItem(Data)
                };
            return null;
        }

        bool IResultItem.Overlapps(SourcePart targetPart) => Data.Intersect(targetPart) != null;
        protected override string GetNodeDump() => "[" + Data.Id + "]";
    }

    sealed class ResultItems : DumpableObject
    {
        readonly List<IResultItem> Data;

        public ResultItems() { Data = new List<IResultItem>(); }
        public ResultItems(IEnumerable<IResultItem> data) { Data = new List<IResultItem>(data); }

        public bool IsEmpty => Data.All(item => item.Text == "");

        public void Add(string text)
        {
            if(text == "")
                return;
            Add(new TextItem(text));
        }

        public void AddHidden(SourcePart sourcePart) => Add(new HiddenSourceItem(sourcePart));
        public void Add(SourcePart sourcePart) => Add(new SourceItem(sourcePart));

        public ResultItems Combine(ResultItems other)
        {
            var result = new ResultItems(Data);
            foreach(var item in other.Data)
                result.Add(item);
            return result;
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

            var start = Data.IndexWhere(item => item is SourceItem && item.Overlapps(targetPart));
            if(start != null)
                data = Data.Skip(start.Value);

            var f = (SourceItem) data.FirstOrDefault();
            if(f == null)
                return result;

            var start1 = targetPart.Position - f.Data.Position;
            foreach(var item in data)
            {
                var t = item as TextItem;
                if(t != null)
                {
                    Tracer.Assert(start1 == 0);
                    result += item.Text;
                }

                var s = item as SourceItem;
                if(s != null)
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

            var start = Data.IndexWhere(item => item is SourceItem && item.Overlapps(targetPart));
            if(start != null)
                data = Data.Skip(start.Value);

            var f = (SourceItem) data.FirstOrDefault();
            if(f == null)
                yield break;

            var newText = "";
            var removeCount = 0;

            foreach(var item in data)
            {
                var t = item as TextItem;
                if(t != null)
                    newText += item.Text;

                var s = item as SourceItem;
                if(s != null)
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

                var h = item as HiddenSourceItem;
                if(h != null && h.Data.Id != "\r")
                {
                    removeCount += h.Data.Length;
                    if(newText != "" || removeCount > 0)
                    {
                        yield return new EditPiece
                        {
                            NewText = newText,
                            RemoveCount = removeCount,
                            Position = h.Data.Position
                        };
                        newText = "";
                        removeCount = 0;
                    }
                }
            }
        }

        string Text { get { return Data.Aggregate("", (c, n) => c + n.Text); } }

        protected override string GetNodeDump() => Data.Count + "->" + Text + "<-";

        internal bool HasInnerLineBreaks()
        {
            return Data.Skip(1).Any(item => item.Text.Contains("\n"));
        }

        public override string ToString() => Text;
    }

    public sealed class EditPiece: DumpableObject
    {
        internal int Position;
        internal int RemoveCount;
        internal string NewText;
    }
}