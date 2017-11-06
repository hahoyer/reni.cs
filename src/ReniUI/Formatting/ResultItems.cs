using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using Reni.Parser;

namespace ReniUI.Formatting
{
    sealed class ResultItems : DumpableObject, IAggregateable<ResultItems>
    {
        public static ResultItems Default() {return new ResultItems();}
        readonly List<IResultItem> Data;

        ResultItems() => Data = new List<IResultItem>();

        ResultItems(IEnumerable<IResultItem> data) => Data = new List<IResultItem>(data);

        ResultItems IAggregateable<ResultItems>.Aggregate(ResultItems other) => Combine(other);

        public bool IsEmpty => Data.All(item => item.VisibleText == "");

        string Text {get {return Data.Aggregate("", (c, n) => c + n.VisibleText);}}

        ResultItems Combine(ResultItems other)
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

        internal IEnumerable<Edit> GetEditPieces(SourcePart targetPart = null)
        {
            if(targetPart == null)
                return Enumerable.Empty<Edit>();

            IEnumerable<IResultItem> data = Data;

            var start = Data.IndexWhere
                (item => item.SourcePart?.Intersect(targetPart) != null);

            if(start != null)
                data = Data.Skip(start.Value);

            var newText = "";
            var removeCount = 0;

            var pieces = new List<Edit>();
            foreach(var item in data)
            {
                newText += item.NewText;
                removeCount += item.RemoveCount;

                var sourcePart = item.SourcePart;
                if(sourcePart == null)
                    continue;

                if(newText != "" || removeCount > 0)
                {
                    pieces.Add(new Edit
                    {
                        Location = (sourcePart.End + -removeCount).Span(removeCount),
                        NewText = newText
                    });

                    newText = "";
                    removeCount = 0;
                }

                if(targetPart.EndPosition <= sourcePart.Position)
                    return pieces;
            }
            return pieces;
        }

        internal IEnumerable<Edit> GetEditPieces1(SourcePart targetPart = null)
        {
            if(targetPart == null)
                yield break;

            IEnumerable<IResultItem> data = Data;

            var start = Data.IndexWhere
                (item => item.SourcePart?.Intersect(targetPart) != null);

            if(start != null)
                data = Data.Skip(start.Value);

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
                    yield return new Edit
                    {
                        Location = (sourcePart.End + -removeCount).Span(removeCount),
                        NewText = newText
                    };

                    newText = "";
                    removeCount = 0;
                }

                if(targetPart.EndPosition <= sourcePart.Position)
                    yield break;
            }
        }

        protected override string GetNodeDump() => Data.Count + "->" + Text + "<-";

        internal bool HasInnerLineBreaks() 
            => Data
            .Skip(1)
            .Any(item => item.VisibleText.Contains("\n"));

        public override string ToString() => Text;
    }
}