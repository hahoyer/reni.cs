using System;
using hw.DebugFormatter;
using JetBrains.Annotations;

// ReSharper disable CheckNamespace

namespace hw.Scanner
{
    [PublicAPI]
    public interface IMatch
    {
        int? Match(SourcePosition sourcePosition, bool isForward = true);
    }

    [PublicAPI]
    public sealed class Match : DumpableObject, IMatch
    {
        public interface IError { }

        public sealed class Exception : System.Exception
        {
            public readonly IError Error;
            public readonly SourcePosition SourcePosition;

            public Exception(SourcePosition sourcePosition, IError error)
            {
                SourcePosition = sourcePosition;
                Error = error;
            }
        }

        sealed class NotMatch : DumpableObject, IMatch
        {
            readonly IMatch Data;
            public NotMatch(IMatch data) => Data = data;

            int? IMatch.Match(SourcePosition sourcePosition, bool isForward)
            {
                var result = Data.Match(sourcePosition, isForward);
                return result != null
                    ? null
                    : isForward
                        ? 1
                        : -1;
            }
        }

        sealed class Sequence : DumpableObject, IMatch
        {
            [EnableDump]
            readonly IMatch Data;

            [EnableDump]
            readonly IMatch Other;

            public Sequence(IMatch data, IMatch other)
            {
                Data = data;
                Other = other;
            }

            int? IMatch.Match(SourcePosition sourcePosition, bool isForward)
            {
                int? result = 0;
                int? otherResult = 0;

                if(isForward)
                {
                    result = Data.Match(sourcePosition);
                    if(result == null)
                        return null;
                }

                otherResult = Other.Match(sourcePosition + result.Value, isForward);
                if(otherResult == null)
                    return null;

                if(!isForward)
                {
                    result = Data.Match(sourcePosition + otherResult.Value, false);
                    if(result == null)
                        return null;
                }

                return result.Value + otherResult.Value;
            }
        }

        sealed class FunctionalMatch : DumpableObject, IMatch
        {
            [EnableDump]
            readonly Func<char, bool> Func;

            [EnableDump]
            readonly bool IsTrue;

            public FunctionalMatch(Func<char, bool> func, bool isTrue)
            {
                Func = func;
                IsTrue = isTrue;
            }

            int? IMatch.Match(SourcePosition sourcePosition, bool isForward)
                => Func((sourcePosition - (isForward? 0 : 1)).Current) != IsTrue
                    ? null
                    : isForward
                        ? 1
                        : -1;
        }

        sealed class FindMatch : DumpableObject, IMatch
        {
            [EnableDump]
            readonly IMatch Data;

            readonly SourcePart Bound;

            public FindMatch(IMatch data, SourcePosition bound = null)
            {
                Data = data;
                Bound = bound == null? null : bound.Span(0);
            }

            int? IMatch.Match(SourcePosition sourcePosition, bool isForward)
            {
                if(Bound != null && sourcePosition.Source != Bound.Source)
                    throw new InvalidOperationException();

                var current = sourcePosition.Clone;
                while(true)
                {
                    var result = Data.Match(current, isForward);
                    if(result != null && (Bound == null || IsBoundReached(isForward, current + result.Value)))
                        return current - sourcePosition + result;

                    if(IsBoundReached(isForward, current))
                        return null;

                    current.Position += isForward? 1 : -1;
                }
            }

            bool IsBoundReached(bool isForward, SourcePosition current)
            {
                var bound = Bound ?? current.Source.All;
                return isForward? current >= bound.End : current <= bound.Start;
            }
        }

        sealed class ValueMatch : DumpableObject, IMatch
        {
            [EnableDump]
            readonly IMatch Data;

            [EnableDump]
            readonly Func<string, IMatch> Func;

            public ValueMatch(IMatch data, Func<string, IMatch> func)
            {
                Data = data;
                Func = func;
            }

            int? IMatch.Match(SourcePosition sourcePosition, bool isForward)
            {
                var length = Data.Match(sourcePosition, isForward);
                if(length == null)
                    return null;

                var value = sourcePosition.SubString(0, length.Value);
                var funcResult = Func(value).Match(sourcePosition + length.Value, isForward);
                return funcResult == null? null : length.Value + funcResult;
            }
        }

        sealed class FrameMatch : DumpableObject, IMatch
        {
            int? IMatch.Match(SourcePosition sourcePosition, bool isForward)
                => isForward && sourcePosition.IsEnd || !isForward && sourcePosition.Position <= 0? 0 : null;
        }

        sealed class BreakMatch : DumpableObject, IMatch
        {
            int? IMatch.Match(SourcePosition sourcePosition, bool isForward)
            {
                Tracer.TraceBreak();
                return 0;
            }
        }


        readonly IMatch Data;

        internal Match(IMatch data)
        {
            (!(data is Match)).Assert();
            Data = data;
        }

        int? IMatch.Match(SourcePosition sourcePosition, bool isForward)
            => Data.Match(sourcePosition, isForward);

        public static Match Break => new(new BreakMatch());

        public static Match WhiteSpace => Box(char.IsWhiteSpace);
        public static Match LineEnd => "\n".Box().Else("\r\n").Else(End);
        public static Match End => new(new FrameMatch());
        public static Match Digit => Box(char.IsDigit);
        public static Match Letter => Box(char.IsLetter);
        public static Match Any => Box(c => true);

        [DisableDump]
        public IMatch UnBox => Data.UnBox();

        [DisableDump]
        public Match Find => new(new FindMatch(Data));

        [DisableDump]
        public Match Not => new(new NotMatch(this));

        public static Match Box(Func<char, bool> func) => new(new FunctionalMatch(func, true));

        public static Match operator +(string target, Match y) => target.Box() + y;
        public static Match operator +(Match target, string y) => target + y.Box();

        public static Match operator +(IError target, Match y) => target.Box() + y;
        public static Match operator +(Match target, IError y) => target + y.Box();

        public static Match operator +(Match target, Match y)
            => new(new Sequence(target.UnBox(), y.UnBox()));

        public static Match operator |(Match target, Match y) => target.Else(y);

        public Match Repeat(int minCount = 0, int? maxCount = null)
            => Data.Repeat(minCount, maxCount);

        public Match Option() => Data.Repeat(maxCount: 1);

        public Match Else(IMatch other) => Data.Else(other);
        public Match Value(Func<string, IMatch> func) => new(new ValueMatch(Data, func));

        public Match FindUntil(SourcePosition end) => new(new FindMatch(Data, end));
    }
}