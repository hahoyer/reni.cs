using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni.Parser;

namespace ReniUI.Formatting
{
    sealed class ResultItems : DumpableObject, IAggregateable<ResultItems>
    {
        readonly List<IResultItem> Data;

        ResultItems() => Data = new();

        ResultItems(IEnumerable<IResultItem> data) => Data = new(data);

        ResultItems IAggregateable<ResultItems>.Aggregate(ResultItems other) => Combine(other);

        protected override string GetNodeDump() => Data.Count + "->" + Text + "<-";

        public override string ToString() => Text;

        public bool IsEmpty => Data.All(item => item.VisibleText == "");

        internal string Text => Data.Aggregate("", (c, n) => c + n.VisibleText);
        public static ResultItems Default() => new();

        static bool CheckForStart(IResultItem item, ref SourcePosition lastPosition, SourcePart targetPart)
        {
            (item != null).Assert();

            var sourcePart = item.SourcePart;

            if(sourcePart == null)
                return false;

            if(lastPosition == null || sourcePart.Intersect(targetPart) == null)
                lastPosition = sourcePart.End;

            return sourcePart.Intersect(targetPart) != null;
        }

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

        internal void AddHidden(IItem token)
        {
            token.IsNonComment().Assert();
            Add(WhiteSpace.Create(token, false));
        }

        internal void Add(SourcePart token) => Add(new Skeleton(token));

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
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
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

        internal IEnumerable<Edit> GetEditPieces(SourcePart targetPart)
        {
            (targetPart != null).Assert();

            var endPosition = targetPart.EndPosition;
            SourcePosition lastPosition = null;
            using(var enumerator = Data.GetEnumerator())
            {
                while(enumerator.MoveNext() && !CheckForStart(enumerator.Current, ref lastPosition, targetPart)) { }

                (lastPosition != null).Assert();

                var result = new List<Edit>();
                var newText = "";
                var removeCount = 0;

                while(enumerator.MoveNext())
                {
                    var item = enumerator.Current;

                    (item != null).Assert();

                    newText += item.NewText;
                    removeCount += item.RemoveCount;

                    if(item.SourcePart == null)
                        continue;

                    if(newText != "" || removeCount > 0)
                    {
                        result.Add(Edit.Create("", lastPosition.Span(removeCount), newText));
                        newText = "";
                        removeCount = 0;
                    }

                    if(endPosition <= lastPosition.Position)
                        return result;

                    lastPosition = item.SourcePart.End;
                }

                return result;
            }
        }

        internal bool HasInnerLineBreaks()
            => Data
                .Skip(1)
                .Any(item => item.VisibleText.Contains("\n"));
    }
}