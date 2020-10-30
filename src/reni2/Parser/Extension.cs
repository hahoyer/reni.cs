using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using JetBrains.Annotations;
using Reni.TokenClasses;
using Reni.Validation;

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

        internal static bool IsRelevantWhitespace(this IEnumerable<IItem> whiteSpaces)
            => whiteSpaces?.Any(item => !Lexer.IsSpace(item)) ?? false;

        internal static bool HasComment(this IEnumerable<IItem> whiteSpaces)
            => whiteSpaces?.Any(IsComment) ?? false;

        internal static bool HasLineComment(this IEnumerable<IItem> whiteSpaces)
            => whiteSpaces?.Any(Lexer.IsLineComment) ?? false;

        internal static SourcePart PrefixCharacters(this IToken token)
            => token.PrecededWith.SourcePart() ?? token.Characters.Start.Span(0);

        internal static bool HasWhiteSpaces(this IEnumerable<IItem> whiteSpaces)
            => whiteSpaces?.Any(Lexer.IsSpace) ?? false;

        internal static bool HasLines(this IEnumerable<IItem> whiteSpaces)
            => whiteSpaces?.Any(HasLines) ?? false;

        internal static bool HasLines(this IItem item) => item.SourcePart.Id.Contains("\n");

        internal static IEnumerable<IItem> OnlyComments(this IEnumerable<IItem> whiteSpaces)
            => whiteSpaces.Where(IsComment);

        internal static IEnumerable<IItem> Trim(this IEnumerable<IItem> whiteSpaces)
        {
            var buffer = new List<IItem>();

            foreach(var whiteSpace in whiteSpaces.SkipWhile(IsNonComment))
                if(IsNonComment(whiteSpace))
                    buffer.Add(whiteSpace);
                else
                {
                    foreach(var item in buffer)
                        yield return item;

                    buffer = new List<IItem>();
                    yield return whiteSpace;
                }
        }

        internal static bool IsNonComment(this IItem item)
            => !IsComment(item);

        internal static bool IsComment(this IItem item)
            => Lexer.IsMultiLineComment(item) || Lexer.IsLineComment(item);

        internal static bool IsMultiLineComment(this IItem item)
            => Lexer.IsMultiLineComment(item) ;

        internal static bool IsLineComment(this IItem item)
            => Lexer.IsLineComment(item);

        internal static bool IsLineEnd(this IItem item)
            => Lexer.IsLineEnd(item);

        internal static string State(this IItem item)
            => Lexer.Instance.WhiteSpaceId(item);

        internal static bool IsSpace(this IItem item)
            => Lexer.IsSpace(item);

        public static int Length(this IEnumerable<Lexer.WhiteSpaceToken> whiteSpaceTokens)
            => whiteSpaceTokens.Sum(item => item.Characters.Id.Length);

        public static string Id(this IEnumerable<Lexer.WhiteSpaceToken> whiteSpaceTokens)
            => whiteSpaceTokens.Select(item => item.Characters.Id).Stringify("");

        internal static IEnumerable<Lexer.WhiteSpaceToken> OnlyLeftPart
            (this IEnumerable<Lexer.WhiteSpaceToken> whiteSpaces)
            => whiteSpaces.Take(ThisLineIndex(whiteSpaces));

        internal static IEnumerable<Lexer.WhiteSpaceToken> OnlyRightPart
            (this IEnumerable<Lexer.WhiteSpaceToken> whiteSpaces)
            => whiteSpaces.Skip(ThisLineIndex(whiteSpaces));

        static int ThisLineIndex(IEnumerable<Lexer.WhiteSpaceToken> whiteSpaces)
        {
            var data = whiteSpaces.ToArray();
            var result = 0;
            for(var i = 0; i < data.Length; i++)
                if(data[i].Characters.Id.Contains("\n"))
                    result = i + 1;

            return result;
        }

        [UsedImplicitly]
        internal static string Symbolize
            (this string token) => token.Aggregate("", (current, tokenChar) => current + SymbolizeChar(tokenChar));

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

        internal static IMatch SavePart(this IMatch target, Action<string> onMatch, Action onMismatch = null)
            => new SavePartMatch(target, onMatch, onMismatch);

        internal static int? Apply(this IMatch pattern, string target)
            => (new Source(target) + 0).Match(pattern);

        internal static Result<TTarget> AddIssues<TTarget>(this TTarget target, params Issue[] issues)
            where TTarget : class
            => new Result<TTarget>(target, issues);

        internal static Result<TResult> Apply<TArg1, TResult>(this Result<TArg1> arg1, Func<TArg1, TResult> creator)
            where TArg1 : class
            where TResult : class
            => creator(arg1?.Target).AddIssues(arg1?.Issues);

        internal static Result<TResult> Apply<TArg1, TArg2, TResult>
            (this(Result<TArg1> arg1, Result<TArg2> arg2) arg, Func<TArg1, TArg2, TResult> creator)
            where TArg1 : class
            where TArg2 : class
            where TResult : class
            => creator(arg.arg1?.Target, arg.arg2?.Target)
                .AddIssues(T(arg.arg1?.Issues, arg.arg2?.Issues).ConcatMany().ToArray());

        internal static Result<TResult> Apply<TArg1, TArg2, TArg3, TResult>
        (
            this(Result<TArg1> arg1, Result<TArg2> arg2, Result<TArg3> arg3) arg
            , Func<TArg1, TArg2, TArg3, TResult> creator
        )
            where TArg1 : class
            where TArg2 : class
            where TResult : class
            where TArg3 : class
            => creator(arg.arg1?.Target, arg.arg2?.Target, arg.arg3?.Target)
                .AddIssues(T(arg.arg1?.Issues, arg.arg2?.Issues, arg.arg3?.Issues).ConcatMany().ToArray());

        internal static Result<TResult> Apply<TArg1, TResult>
            (this Result<TArg1> arg1, Func<TArg1, Result<TResult>> creator)
            where TArg1 : class
            where TResult : class
            => creator(arg1?.Target).With(arg1?.Issues);

        internal static Result<TResult> Apply<TArg1, TArg2, TResult>
            (this(Result<TArg1> arg1, Result<TArg2> arg2) arg, Func<TArg1, TArg2, Result<TResult>> creator)
            where TArg1 : class
            where TArg2 : class
            where TResult : class
            => creator(arg.arg1?.Target, arg.arg2?.Target)
                .With(T(arg.arg1?.Issues, arg.arg2?.Issues).ConcatMany().ToArray());

        internal static Result<TResult> Apply<TArg1, TArg2, TArg3, TResult>
        (
            this(Result<TArg1> arg1, Result<TArg2> arg2, Result<TArg3> arg3) arg
            , Func<TArg1, TArg2, TArg3, Result<TResult>> creator
        )
            where TArg1 : class
            where TArg2 : class
            where TArg3 : class
            where TResult : class
            => creator(arg.arg1?.Target, arg.arg2?.Target, arg.arg3?.Target)
                .With(T(arg.arg1?.Issues, arg.arg2?.Issues, arg.arg3?.Issues).ConcatMany().ToArray());

        [UsedImplicitly]
        internal static TValue[] T<TValue>(params TValue[] value) => value;

        internal static BinaryTree[] Combine(this IEnumerable<IEnumerable<BinaryTree>> targets)
        {
            var target = targets.ConcatMany().ToArray();
            return target
                .Where(b => !target.Any(p => p != b && p.SourcePart.Contains(b.SourcePart)))
                .ToArray();
        }

        static string GetDumpAfterCurrent(this SourcePart target, int dumpWidth)
        {
            if(target.Source.IsEnd(target.EndPosition))
                return "";

            var length = Math.Min(dumpWidth, target.Source.Length - target.EndPosition);
            var result = target.Source.SubString(target.EndPosition, length);
            if(length == dumpWidth)
                result += "...";
            return result;
        }

        static string GetDumpBeforeCurrent(this SourcePart target, int dumpWidth)
        {
            var delta = target.Position - dumpWidth;
            var start = delta < 3? 0 : delta;
            return (delta < 3? "" : "...") + target.Source.SubString(start, target.Position - start);
        }

        public static string DumpSource(this SourcePart[] target, int dumpWidth = 5)
        {
            if(!target.Any())
                return "";

            target = target
                .OrderBy(item => item.Position)
                .ThenBy(item => item.EndPosition)
                .ToArray();

            var result = target[0].GetDumpBeforeCurrent(dumpWidth) + "[";
            for(var index = 0; index < target.Length; index++)
                result += DumpSource(target, index, dumpWidth);

            return result;
        }

        static string DumpSource(SourcePart[] target, int index, int dumpWith)
        {
            var current = target[index];
            var next = index + 1 < target.Length? target[index + 1] : null;

            var result = current.Id +"]";
            var delta = (next == null? current.Source.Length : next.Position) - current.EndPosition;

            if(next == null)
            {
                if(delta < dumpWith + 3)
                    return result + current.End.Span(delta).Id;
                return result  + current.End.Span(dumpWith).Id + "...";
            }

            (current.Source == next.Source).Assert();

            if(delta < 0)
                return current.Start.Span(next.Start).Id;

            if(delta < 2 * dumpWith + 3)
                return result + current.End.Span(next.Start).Id + "[";

            return result +
                   current.End.Span(dumpWith).Id +
                   "..." +
                   (next.Start + -dumpWith).Span(dumpWith).Id +
                   "[";
        }

        internal static SourcePart Combine(this IEnumerable<SourcePart> target1)
        {
            var target = target1.ToArray();

            if(!target.Any())
                return null;

            target.All(item => item.Source == target[0].Source).Assert();

            var start = target.Select(item => item.Position).Min();
            var end = target.Select(item => item.EndPosition).Max();
            return new SourcePosition(target[0].Source, start).Span(end - start);
        }
    }
}