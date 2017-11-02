using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using Reni.Parser;

namespace ReniUI.Formatting {
    sealed class ResultItems : DumpableObject
    {
        readonly List<IResultItem> Data;

        public ResultItems() => Data = new List<IResultItem>();
        public ResultItems(IEnumerable<IResultItem> data) => Data = new List<IResultItem>(data);

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

        internal void Add(IItem token)
            => Add
            (
                token.IsComment()
                    ? (IResultItem) new Skeleton(token.SourcePart)
                    : WhiteSpace.Create(token, true)
            );

        internal void AddHidden(IItem token)
        {
            Tracer.Assert(token.IsNonComment());
            Add(WhiteSpace.Create(token, false));
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
                        EndPosition = sourcePart.Position
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
}