using hw.Parser;
using hw.Scanner;
using Reni.Context;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.Parser;

static class Extension
{
    [UsedImplicitly]
    internal static string Symbolize
        (this string token) => token.Aggregate("", (current, tokenChar) => current + SymbolizeChar(tokenChar));

    static string SymbolizeChar(char @char)
    {
        switch(@char)
        {
            case '§':
                return "Paragraph";
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
                return "Bad" + Convert.ToByte((int)@char).ToString("x2");
        }
    }

    [UsedImplicitly]
    internal static TValue[] T1<TValue>(params TValue[] value) => value;

    static string DumpSource(SourcePart[] target, int index, int dumpWith)
    {
        target = target
            .OrderBy(node => node.Position)
            .ThenBy(node => node.EndPosition)
            .ToArray();

        var current = target[index];
        var next = index + 1 < target.Length? target[index + 1] : null;

        var result = current.Id + "]";
        var delta = (next == null? current.Source.Length : next.Position) - current.EndPosition;

        if(next == null)
        {
            if(delta < dumpWith + 3)
                return result + current.End.Span(delta).Id;
            return result + current.End.Span(dumpWith).Id + "...";
        }

        (current.Source == next.Source).Assert();

        if(delta < 0)
            return current.Start.Span(next.Start).Id;

        if(delta < 2 * dumpWith + 3)
            return result + current.End.Span(next.Start).Id + "[";

        return result + current.End.Span(dumpWith).Id + "..." + (next.Start + -dumpWith).Span(dumpWith).Id + "[";
    }

    extension<T>(IEnumerable<T>? x)
    {
        internal T[] Plus(IEnumerable<T>? y)
            => (x ?? new T[0])
                .Concat(y ?? new T[0])
                .ToDistinctNotNullArray();

        internal T[] ToDistinctNotNullArray()
            => (x ?? new T[0]).Where(item => item != null).Distinct().ToArray();
    }

    extension(IEnumerable<IItem>? whiteSpaces)
    {
        internal bool IsRelevantWhitespace
            => whiteSpaces?.Any(item => !Lexer.IsSpace(item)) ?? false;

        internal bool HasComment
            => whiteSpaces?.Any(IsComment) ?? false;

        internal bool HasLineComment
            => whiteSpaces?.Any(Lexer.IsLineComment) ?? false;

        internal bool HasMultiLineComment
            => whiteSpaces?.Any(Lexer.IsMultiLineComment) ?? false;

        internal bool HasWhiteSpaces
            => whiteSpaces?.Any(Lexer.IsSpace) ?? false;

        internal IEnumerable<IItem> OnlyComments
            => whiteSpaces?.Where(IsComment)?? [];

        internal IEnumerable<IItem> Trim
        {
            get
            {
                var buffer = new List<IItem>();

                foreach(var whiteSpace in whiteSpaces?.SkipWhile(IsNonComment)??[])
                    if(IsNonComment(whiteSpace))
                        buffer.Add(whiteSpace);
                    else
                    {
                        foreach(var item in buffer)
                            yield return item;

                        buffer = new();
                        yield return whiteSpace;
                    }
            }
        }
    }

    extension(IItem item)
    {
        internal bool HasLineAtEnd() => item.IsLineEnd() || item.IsLineComment();

        internal bool IsNonComment()
            => !IsComment(item);

        internal bool IsComment()
            => Lexer.IsMultiLineComment(item) || Lexer.IsLineComment(item);

        internal bool IsMultiLineComment()
            => Lexer.IsMultiLineComment(item);

        internal bool IsMultiLineCommentEnd()
            => Lexer.IsMultiLineCommentEnd(item);

        internal bool IsLineComment()
            => Lexer.IsLineComment(item);

        internal bool IsLineEnd()
            => Lexer.IsLineEnd(item);

        internal bool IsSpace()
            => Lexer.IsSpace(item);
    }

    extension(SourcePart prefix)
    {
        internal bool? SeparatorRequest
        {
            get
            {
                if(prefix.Position == 0 || prefix.End.IsEnd)
                    return false;
                Dumpable.NotImplementedFunction(prefix);
                return default;
            }
        }

        [PublicAPI]
        string GetDumpAfterCurrent(int dumpWidth)
            => prefix.Source.GetDumpAfterCurrent(prefix.EndPosition, dumpWidth);

        string GetDumpBeforeCurrent(int dumpWidth)
            => prefix.Source.GetDumpBeforeCurrent(prefix.Position, dumpWidth);
    }

    extension(IMatch target)
    {
        internal IMatch SavePart(Action<string> onMatch, Action? onMismatch = null)
            => new SavePartMatch(target, onMatch, onMismatch);

        internal int? Apply(string target1)
            => (new Source(target1) + 0).Match(target);
    }

    extension<TArg1>(Result<TArg1>? arg1)
        where TArg1 : class
    {
        internal Result<TResult> Apply<TResult>(Func<TArg1?, TResult> creator)
            where TResult : class
            => creator(arg1?.Target).AddIssues(arg1?.Issues);

        internal Result<TResult> Apply<TResult>
            (Func<TArg1?, Result<TResult>> creator)
            where TResult : class
            => creator(arg1?.Target).With(arg1?.Issues);
    }

    extension<TArg1, TArg2>((Result<TArg1>? arg1, Result<TArg2>? arg2) arg)
        where TArg1 : class
        where TArg2 : class
    {
        internal Result<TResult> Apply<TResult>
            (Func<TArg1?, TArg2?, TResult> creator)
            where TResult : class
            => creator(arg.arg1?.Target, arg.arg2?.Target)
                .AddIssues(T1(arg.arg1?.Issues, arg.arg2?.Issues).ConcatMany().ToArray());

        internal Result<TResult> Apply<TResult>
            (Func<TArg1?, TArg2?, Result<TResult>> creator)
            where TResult : class
            => creator(arg.arg1?.Target, arg.arg2?.Target)
                .With(T1(arg.arg1?.Issues, arg.arg2?.Issues).ConcatMany().ToArray());
    }

    extension<TArg1, TArg2, TArg3>((Result<TArg1>? arg1, Result<TArg2>? arg2, Result<TArg3>? arg3) arg)
        where TArg1 : class
        where TArg2 : class
        where TArg3 : class
    {
        internal Result<TResult> Apply<TResult>
        (
            Func<TArg1?, TArg2?, TArg3?, TResult> creator
        )
            where TResult : class
            => creator(arg.arg1?.Target, arg.arg2?.Target, arg.arg3?.Target)
                .AddIssues(T1(arg.arg1?.Issues, arg.arg2?.Issues, arg.arg3?.Issues).ConcatMany().ToArray());

        internal Result<TResult> Apply<TResult>
        (
            Func<TArg1?, TArg2?, TArg3?, Result<TResult>> creator
        )
            where TResult : class
            => creator(arg.arg1?.Target, arg.arg2?.Target, arg.arg3?.Target)
                .With(T1(arg.arg1?.Issues, arg.arg2?.Issues, arg.arg3?.Issues).ConcatMany().ToArray());
    }

    extension(BinaryTree[] targets)
    {
        public SourcePart[] SourceParts => targets.Select(item => item.FullToken).ToArray();
        public Root Root => targets.Select(item => item.Root).Distinct().Single();
    }

    extension<T>(IEnumerable<T>? x)
        where T : class
    {
        internal T[] Plus(T y)
            => (x ?? new T[0])
                .Concat(y.NullableToArray())
                .ToDistinctNotNullArray();
    }

    extension<T>(T x)
    {
        internal T[] Plus(IEnumerable<T> y)
            => new[] { x }.Concat(y).ToDistinctNotNullArray();
    }

    extension(IToken token)
    {
        internal bool HasComment()
            => Lexer.Instance.HasComment(token.GetPrefixSourcePart());
    }

    extension<TTarget>(TTarget target)
        where TTarget : class
    {
        internal Result<TTarget> AddIssues(params Issue[]? issues) => new(target, issues);
    }

    extension(IEnumerable<IEnumerable<BinaryTree>> targets)
    {
        internal BinaryTree[] Combine()
        {
            var target = targets.ConcatMany().ToArray();
            return target
                .Where(b => !target.Any(p => p != b && p.SourcePart.Contains(b.SourcePart)))
                .ToArray();
        }
    }

    extension(IEnumerable<SourcePart> target1)
    {
        internal SourcePart? Combine()
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

    extension(SourcePart[] target)
    {
        public string DumpSource(int dumpWidth = 5)
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
    }
}