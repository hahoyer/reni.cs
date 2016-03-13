using System;
using System.Collections.Generic;
using System.Linq;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using JetBrains.Annotations;

namespace Reni.Parser
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
            => whiteSpaces?.Any(item => !Lexer.IsWhiteSpace(item)) ?? false;

        internal static bool HasComment(this IEnumerable<WhiteSpaceToken> whiteSpaces)
            => whiteSpaces?.Any(IsComment) ?? false;

        internal static bool HasLineComment(this IEnumerable<WhiteSpaceToken> whiteSpaces)
            => whiteSpaces?.Any(Lexer.IsLineComment) ?? false;

        internal static SourcePart PrefixCharacters(this IToken token)
            => token.PrecededWith.SourcePart() ?? token.Characters.Start.Span(0);

        internal static bool HasWhiteSpaces(this IEnumerable<WhiteSpaceToken> whiteSpaces)
            => whiteSpaces?.Any(Lexer.IsWhiteSpace) ?? false;

        internal static bool HasLines(this IEnumerable<WhiteSpaceToken> whiteSpaces)
            => whiteSpaces?.Any(item => item.Characters.Id.Contains("\n")) ?? false;

        internal static IEnumerable<WhiteSpaceToken> OnlyComments
            (this IEnumerable<WhiteSpaceToken> whiteSpaces)
            => whiteSpaces.Where(IsComment);

        internal static IEnumerable<WhiteSpaceToken> Trim
            (this IEnumerable<WhiteSpaceToken> whiteSpaces)
        {
            var buffer = new List<WhiteSpaceToken>();

            foreach(var whiteSpace in whiteSpaces.SkipWhile(IsNonComment))
                if(IsNonComment(whiteSpace))
                    buffer.Add(whiteSpace);
                else
                {
                    foreach(var item in buffer)
                        yield return item;
                    buffer = new List<WhiteSpaceToken>();
                    yield return whiteSpace;
                }
        }

        internal static bool IsNonComment(this WhiteSpaceToken item)
            => !IsComment(item);

        internal static bool IsComment(this WhiteSpaceToken item)
            => Lexer.IsComment(item) || Lexer.IsLineComment(item);

        internal static bool IsLineBreak(this WhiteSpaceToken item)
            => Lexer.IsLineEnd(item);

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

        [UsedImplicitly]
        internal static string Symbolize(this string token)
        {
            return token.Aggregate("", (current, tokenChar) => current + SymbolizeChar(tokenChar));
        }

        static string SymbolizeChar(char @char)
        {
            switch(@char)
            {
            case '&':
                return "And";
            case '\\':
                return "Backslash";
            case ':':
                return "Colon";
            case '.':
                return "Dot";
            case '=':
                return "Equal";
            case '>':
                return "Greater";
            case '<':
                return "Less";
            case '-':
                return "Minus";
            case '!':
                return "Not";
            case '|':
                return "Or";
            case '+':
                return "Plus";
            case '/':
                return "Slash";
            case '*':
                return "Star";
            case '~':
                return "Tilde";
            case ' ':
                return "Space";
            case '_':
                return "__";
            default:
                if(char.IsLetter(@char))
                    return "_" + @char;
                if(char.IsDigit(@char))
                    return @char.ToString();
                throw new NotImplementedException("Symbolize(" + @char + ")");
            }
        }

        internal static IMatch SavePart
            (this IMatch target, Action<string> onMatch, Action onMismatch = null)
            => new SavePartMatch(target, onMatch, onMismatch);

        internal static int? Apply(this IMatch pattern, string target)
            => (new Source(target) + 0).Match(pattern);
    }
}