using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
using Reni.TokenClasses;

namespace ReniUI
{
    static class Extension
    {
        const int DefaultTextSize = 10;

        public interface IClickHandler
        {
            void Signal(object target);
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

        sealed class QueryClass<T> : IEnumerable<T>
        {
            readonly Func<IEnumerable<T>> Function;
            public QueryClass(Func<IEnumerable<T>> function) => Function = function;
            IEnumerator<T> GetEnumerator() => Function().GetEnumerator();
            IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        internal static SourcePart Combine(this IEnumerable<SourcePart> target) 
            => SourcePart.SaveCombine(target).Single();
    }
}