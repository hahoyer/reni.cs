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
        public static ResultItems Default() => new ResultItems();

        static bool CheckForStart(IResultItem item, ref SourcePosn lastPosition, SourcePart targetPart)
        {
            Tracer.Assert(item != null);

            var sourcePart = item.SourcePart;

            if(sourcePart == null)
                return false;

            if(lastPosition == null || sourcePart.Intersect(targetPart) == null)
                lastPosition = sourcePart.End;

            return sourcePart.Intersect(targetPart) != null;
        }

        readonly List<IResultItem> Data;

        ResultItems() => Data = new List<IResultItem>();

        ResultItems(IEnumerable<IResultItem> data) => Data = new List<IResultItem>(data);

        ResultItems IAggregateable<ResultItems>.Aggregate(ResultItems other) => Combine(other);

        public bool IsEmpty => Data.All(item => item.VisibleText == "");

        internal string Text {get {return Data.Aggregate("", (c, n) => c + n.VisibleText);}}

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

        internal IEnumerable<Edit> GetEditPieces(SourcePart targetPart)
        {
            Tracer.Assert(targetPart != null);

            var endPosition = targetPart.EndPosition;
            SourcePosn lastPosition = null;
            using(var enumerator = Data.GetEnumerator())
            {
                while(enumerator.MoveNext() && !CheckForStart(enumerator.Current, ref lastPosition, targetPart))
                    ;

                Tracer.Assert(lastPosition != null);

                var result = new List<Edit>();
                var newText = "";
                var removeCount = 0;

                while(enumerator.MoveNext())
                {
                    var item = enumerator.Current;

                    Tracer.Assert(item != null);

                    newText += item.NewText;
                    removeCount += item.RemoveCount;

                    if(item.SourcePart == null)
                        continue;

                    if(newText != "" || removeCount > 0)
                    {
                        result.Add
                        (
                            new Edit
                            {
                                Location = lastPosition.Span(removeCount),
                                NewText = newText
                            });

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

        protected override string GetNodeDump() => Data.Count + "->" + Text + "<-";

        internal bool HasInnerLineBreaks()
            => Data
                .Skip(1)
                .Any(item => item.VisibleText.Contains("\n"));

        public override string ToString() => Text;
    }
}