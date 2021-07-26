using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;

namespace ReniUI
{
    static class Extension
    {
        public interface IClickHandler
        {
            void Signal(object target);
        }

        sealed class QueryClass<T> : IEnumerable<T>
        {
            readonly Func<IEnumerable<T>> Function;
            public QueryClass(Func<IEnumerable<T>> function) => Function = function;
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
            IEnumerator<T> GetEnumerator() => Function().GetEnumerator();
        }

        const int DefaultTextSize = 10;

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
            => SourcePart.SaveCombine(target).Single();
    }
}