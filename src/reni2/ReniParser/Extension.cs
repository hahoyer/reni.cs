using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using hw.Parser;
using Reni.Parser;

namespace Reni.ReniParser
{
    static class Extension
    {
        internal static T[] plus<T>(this IEnumerable<T> x, IEnumerable<T> y)
            => (x ?? new T[0])
                .Concat(y ?? new T[0])
                .ToDistinctNotNullArray();

        internal static T[] plus<T>(this IEnumerable<T> x, T y)
            where T : class
            => (x ?? new T[0])
                .Concat(y.NullableToArray())
                .ToDistinctNotNullArray();

        internal static T[] plus<T>(this T x, IEnumerable<T> y)
            => new[] {x}.Concat(y).ToDistinctNotNullArray();

        internal static T[] ToDistinctNotNullArray<T>(this IEnumerable<T> y)
            => (y ?? new T[0]).Where(item => item != null).Distinct().ToArray();

        internal static bool IsRelevantWhitespace(this IEnumerable<WhiteSpaceToken> whiteSpaces)
            => whiteSpaces?.Any(item => !ReniLexer.IsWhiteSpace(item)) ?? false;

        internal static bool HasComment(this IEnumerable<WhiteSpaceToken> whiteSpaces)
            =>
                whiteSpaces?.Any(item => ReniLexer.IsLineComment(item) || ReniLexer.IsComment(item))
                    ?? false;

        internal static bool HasLines(this IEnumerable<WhiteSpaceToken> whiteSpaces)
            => whiteSpaces?.Any(item => item.Characters.Id.Contains("\n")) ?? false;

        internal static IEnumerable<WhiteSpaceToken> OnlyComments
            (this IEnumerable<WhiteSpaceToken> whiteSpaces)
        {
            if
                (
                    whiteSpaces
                        .Any(item => ReniLexer.IsLineComment(item) || ReniLexer.IsComment(item))
                )
                return whiteSpaces;

            return new WhiteSpaceToken[0];
        }

        public static int Length(this IEnumerable<WhiteSpaceToken> whiteSpaceTokens)
            => whiteSpaceTokens.Sum(item => item.Characters.Id.Length);

        public static string Id(this IEnumerable<WhiteSpaceToken> whiteSpaceTokens)
            => whiteSpaceTokens.Select(item => item.Characters.Id).Stringify("");

        internal static IEnumerable<WhiteSpaceToken> OnlyLeftPart
            (this IEnumerable<WhiteSpaceToken> whiteSpaces)
            => whiteSpaces.Take(ThisLineIndex(whiteSpaces));

        internal static IEnumerable<WhiteSpaceToken> OnlyRightPart
            (this IEnumerable<WhiteSpaceToken> whiteSpaces)
            => whiteSpaces.Skip(ThisLineIndex(whiteSpaces));

        static int ThisLineIndex(IEnumerable<WhiteSpaceToken> whiteSpaces)
        {
            var data = whiteSpaces.ToArray();
            var result = 0;
            for(var i = 0; i < data.Length; i++)
                if(data[i].Characters.Id.Contains("\n"))
                    result = i + 1;
            return result;
        }
    }
}