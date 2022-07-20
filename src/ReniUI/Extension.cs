using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
using Reni.TokenClasses.Whitespace;

namespace ReniUI;

static class Extension
{
    sealed class QueryClass<T> : IEnumerable<T>
    {
        readonly Func<IEnumerable<T>> Function;
        public QueryClass(Func<IEnumerable<T>> function) => Function = function;
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
        IEnumerator<T> GetEnumerator() => Function().GetEnumerator();
    }

    internal static string FilePosition(this SourcePart sourcePart)
    {
        var source = sourcePart.Source;
        var position = sourcePart.Position;
        var positionEnd = sourcePart.EndPosition;
        return source.Identifier +
            "(" +
            (source.LineIndex(position) + 1) +
            "," +
            (source.ColumnIndex(position) + 1) +
            "," +
            (source.LineIndex(positionEnd) + 1) +
            "," +
            (source.ColumnIndex(positionEnd) + 1) +
            ")";
    }

    internal static IEnumerable<T> Query<T>(Func<IEnumerable<T>> function)
        => new QueryClass<T>(function);

    internal static SourcePart Combine(this IEnumerable<SourcePart> target)
        => hw.Scanner.SourcePart.SaveCombine(target).Single();

    internal static IItem LocateToken(this SourcePart prefix, SourcePosition offset)
    {
        if(prefix.Length == 0)
            return null;
        Dumpable.NotImplementedFunction(prefix, offset);
        return default;
    }

    internal static bool HasStableLineBreak(this SourcePart prefix)
    {
        Dumpable.NotImplementedFunction(prefix);
        return default;
    }

    [Obsolete("", true)]
    internal static bool HasLines(this IEnumerable<IItem> whiteSpaces)
        => whiteSpaces?.Any(HasLines) ?? false;

    [Obsolete("", true)]
    internal static bool HasLines(this IItem item)
        => item.ScannerTokenType is ILineBreak;

    internal static SourcePart SourcePart(this IItem item)
    {
        Dumpable.NotImplementedFunction(item);
        return default;
    }

    internal static SourcePart SourcePart(this IEnumerable<IItem> items)
    {
        Dumpable.NotImplementedFunction(items);
        return default;
    }
}