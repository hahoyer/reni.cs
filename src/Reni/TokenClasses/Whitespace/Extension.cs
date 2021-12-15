using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;

namespace Reni.TokenClasses.Whitespace
{
    static class Extension
    {
        internal static( WhiteSpaceItem[][] Items, WhiteSpaceItem[] Tail) SplitAndTail
            (this IEnumerable<WhiteSpaceItem> items, Func<WhiteSpaceItem, bool> tailCondition)
        {
            if(items == null || !items.Any())
                return (new WhiteSpaceItem[][] { }, new WhiteSpaceItem[0]);

            var result = items.Split(tailCondition, false)
                .Select(items => items.ToArray())
                .ToArray();

            var tail = result.Last();
            return tailCondition(tail.Last())
                ? (result, new WhiteSpaceItem[0])
                : (result.Take(result.Length - 1).ToArray(), tail);
        }

        public static IEnumerable<Edit> GetSpaceEdits(this WhiteSpaceItem[] items, bool isSeparatorRequired)
        {
            var delta = (isSeparatorRequired? 1 : 0) - items.Length;

            switch(delta)
            {
                case < 0:
                    var start = items[0].SourcePart.Start;
                    var end = -delta < items.Length
                        ? items[-delta].SourcePart.Start
                        : items[-delta - 1].SourcePart.End;

                    yield return new(start.Span(end), "", "-items/+separator");
                    break;
                case > 0:
                    (!items.Any()).Assert();
                    // because of lacking an anchor this has to be handled outside. 
                    break;
            }
        }

        public static IEnumerable<Edit> GetLineEdits(this LineGroup[] lineGroups, int indent, bool isSeparatorRequired)
        {
            if(!lineGroups.Any())
                return Enumerable.Empty<Edit>();

            var configuration = lineGroups[0].Configuration;
            var targetLineCount = configuration.EmptyLineLimit ?? lineGroups.Length;
            var delta = targetLineCount - lineGroups.Length;
            var result = new List<Edit>();
            switch(delta)
            {
                case < 0:
                {
                    var start = lineGroups[0].SourcePart.Start;
                    var end = -delta < lineGroups.Length
                        ? lineGroups[-delta].Main.SourcePart.Start
                        : lineGroups[-delta - 1].Main.SourcePart.End;

                    result.Add(new(start.Span(end), "", "-extra Linebreaks"));
                    break;
                }
                case > 0:
                    Dumpable.NotImplementedFunction(indent);
                    break;
            }

            result.AddRange(lineGroups.Skip(Dumpable.T(0, -delta - 1).Max()).SelectMany(item => item.GetEdits()));
            if(!isSeparatorRequired || targetLineCount != 0)
                return result;

            result.Add(new(result.Last().Remove.End.Span(0), " ", "+separator(from line)"));
            return result;
        }
    }
}