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

        static TValue[] T<TValue>(params TValue[] value) => value;
    }
}