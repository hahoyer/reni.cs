using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;

namespace Reni.ReniParser
{
    static class Extension
    {
        internal static T[] plus<T>(this T x, params IEnumerable<T>[] y)
            => new[] {x}.Concat(y.SelectMany(item => item)).ToDistinctNotNullArray();

        internal static T[] plus<T>(this IEnumerable<T> x, IEnumerable<T> y)
            => (x ?? new T[0])
                .Concat(y ?? new T[0])
                .ToDistinctNotNullArray();

        internal static T[] plus<T>(this IEnumerable<T> x, T y)
            where T : class
            => (x ?? new T[0])
                .Concat(y.NullableToArray())
                .ToDistinctNotNullArray();

        internal static T[] plus<T>(this T x, params T[] y)
            => new[] {x}.Concat(y).ToDistinctNotNullArray();

        internal static T[] ToDistinctNotNullArray<T>(this IEnumerable<T> y)
            => (y).Where(item => item != null).Distinct().ToArray();
    }
}