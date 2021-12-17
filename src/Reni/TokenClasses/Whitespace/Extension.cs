using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;

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

        public static IEnumerable<Edit> GetSpaceEdits(this SourcePart spaces, int targetCount)
        {
            if(spaces == null)
            {
                (targetCount == 0).Assert();
                yield break;
            }

            var delta = targetCount - spaces.Length;
            if(delta == 0)
                yield break;
            
            yield return new
            (
                spaces.End.Span(T(delta, 0).Min()),
                " ".Repeat(T(delta, 0).Max()),
                "+/-spaces"
            );
        }

        public static IEnumerable<Edit> GetLineEdits(this LineGroup[] lineGroups)
        {
            if(!lineGroups.Any())
                yield break;

            var configuration = lineGroups[0].Configuration;

            var targetLineCount
                = T(configuration.EmptyLineLimit ?? lineGroups.Length, configuration.MinimalLineBreakCount)
                    .Min();

            var delta = targetLineCount - lineGroups.Length;
            switch(delta)
            {
                case < 0:
                {
                    var start = lineGroups[0].SourcePart.Start;
                    var end = -delta < lineGroups.Length
                        ? lineGroups[-delta].Main.SourcePart.Start
                        : lineGroups[-delta - 1].Main.SourcePart.End;

                    yield return new(start.Span(end), "", "-extra Linebreaks");
                    break;
                }
                case > 0:
                    Dumpable.NotImplementedFunction();
                    break;
            }
        }

        static TValue[] T<TValue>(params TValue[] value) => value;
    }
}