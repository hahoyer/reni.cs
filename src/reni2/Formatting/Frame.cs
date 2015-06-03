using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using JetBrains.Annotations;
using Reni.Parser;
using Reni.TokenClasses;

namespace Reni.Formatting
{
    sealed class Frame : DumpableObject
    {
        readonly ValueCache<Frame> _leftCache;
        readonly ValueCache<Frame> _rightCache;
        readonly ValueCache<Frame> _leftNeighborCache;
        readonly ValueCache<Item> _itemCache;
        readonly ValueCache<IEnumerable<Item>> _itemsCache;
        readonly ValueCache<int> _leadingLineBreaksCache;

        [DisableDump]
        internal readonly SmartFormat Formatter;
        [DisableDump]
        internal readonly Frame Parent;
        internal readonly SourceSyntax Target;

        internal Frame(SourceSyntax target, Frame parent = null, SmartFormat formatter = null)
        {
            Parent = parent;
            Formatter = parent?.Formatter ?? formatter;
            Target = target;
            _leftCache = new ValueCache<Frame>(() => new Frame(Target.Left, this));
            _rightCache = new ValueCache<Frame>(() => new Frame(Target.Right, this));
            _leftNeighborCache = new ValueCache<Frame>
                (() => Left.RightMostTokenClassFrame ?? LeftTokenClassFrame);
            _itemsCache = new ValueCache<IEnumerable<Item>>(GetItems);
            _itemCache = new ValueCache<Item>(() => new Item(Whitespaces, Target.Token));
            _leadingLineBreaksCache = new ValueCache<int>(GetLeadingLineBreaksForCache);
        }

        protected override string GetNodeDump()
            => base.GetNodeDump() + "(" + Target?.TokenClass.GetType().PrettyName() + ")";

        [DisableDump]
        Frame Left => _leftCache.Value;

        [DisableDump]
        Frame Right => _rightCache.Value;

        [DisableDump]
        Item Item => _itemCache.Value;

        [DisableDump]
        IEnumerable<Item> Items => _itemsCache.Value;

        [DisableDump]
        Frame LeftNeighbor => _leftNeighborCache.Value;

        [DisableDump]
        string Whitespaces => InternalGetWhitespaces(LeadingLineBreaks, IndentLevel);

        [DisableDump]
        ITokenClass LeftTokenClass => LeftNeighbor?.Target.TokenClass;

        [DisableDump]
        Frame RightMostTokenClassFrame
            =>
                Target == null
                    ? null
                    : Target.Right == null ? this : Right.RightMostTokenClassFrame;

        [DisableDump]
        Frame LeftTokenClassFrame
            => Parent == null || Target == Parent.Target.Right
                ? Parent
                : Parent.LeftTokenClassFrame;

        [DisableDump]
        bool FormatsToMultiline
        {
            get
            {
                var ll = LineLength;
                return ll == null || ll.Value >= Formatter.MaxLineLength;
            }
        }

        Situation GetSituation(Rulers rulers)
        {
            if(Target == null)
                return Situation.Empty;

            if(IsLineBreakRuler)
                return GetRulerSituation(rulers, true).Combine
                    (this, GetRulerSituation(rulers, false));

            return PairSituation(rulers);
        }

        Situation PairSituation(Rulers rulers)
            => Left.GetSituation(rulers) + (GetTokenSituation(rulers) + Right.GetSituation(rulers));

        Situation GetRulerSituation(Rulers rulers, bool isMultiLine)
            => PairSituation(rulers.Concat(this, isMultiLine));

        Situation GetTokenSituation(Rulers rulers)
        {
            var lengths = GetWhitespaceSituationString(rulers).Split('\n').Select(item => item.Length);
            return Situation
                .Create(lengths)
                + Target.Token.Characters.Length;
        }

        bool HasLeadingLineBreaks(Rulers rulers)
        {
            var rightToken = Target.TokenClass;

            if(rightToken is List)
                return false;

            if(rightToken is RightParenthesis)
                return rulers.IsMultiLine(this);

            var leftNeighbor = LeftNeighbor;
            var leftToken = leftNeighbor?.Target.TokenClass;

            if(leftToken is List)
                return rulers.IsMultiLine(leftNeighbor);

            if(leftToken is RightParenthesis)
                return rulers.IsMultiLine(leftNeighbor);

            if(rightToken is LeftParenthesis)
                return rulers.IsMultiLine(this);

            if(leftToken is LeftParenthesis)
                return rulers.IsMultiLine(leftNeighbor);

            return false;
        }

        string GetWhitespaceSituationString(Rulers rulers)
            => InternalGetWhitespaces(HasLeadingLineBreaks(rulers) ? 1 : 0, 0);

        [DisableDump]
        internal Frame LineBreakRuler => IsLineBreakRuler ? this : Parent?.LineBreakRuler;

        [DisableDump]
        bool IsLineBreakRuler
            => Target.TokenClass is EndToken || Target.TokenClass is RightParenthesis;

        int GetLeadingLineBreaksForCache()
        {
            if(Target.TokenClass is List)
                return 0;

            if(Target.TokenClass is RightParenthesis)
                return Left?.RightBraceMode?.Mode.LeadingLineBreaks ?? 0;

            var leftNeighbor = LeftNeighbor;

            if(leftNeighbor?.Target.TokenClass is List)
            {
                var listMode = leftNeighbor.RootListFrame.ListMode;
                if(listMode == null)
                    return 0;

                if(leftNeighbor.Left.FormatsToMultiline)
                    return 2;

                var current = leftNeighbor.Right;
                if(current.Target?.TokenClass is List)
                    current = current.Left;
                return current.FormatsToMultiline ? 2 : 1;
            }

            if(leftNeighbor?.Target.TokenClass is RightParenthesis)
                return leftNeighbor.Left?.RightBraceMode?.Mode.LeadingLineBreaks ?? 0;

            if(Target.TokenClass is LeftParenthesis)
                return Right?.BraceMode?.Mode.LeadingLineBreaks ?? 0;

            if(leftNeighbor?.Target.TokenClass is LeftParenthesis)
                return leftNeighbor.Right?.BraceMode?.Mode.LeadingLineBreaks ?? 0;

            return 0;
        }

        string InternalGetWhitespaces(int leadingLineBreaks, int indentLevel)
        {
            var indent = " ".Repeat(indentLevel * 4);
            var result = "\n".Repeat(leadingLineBreaks);
            var emptyLines = leadingLineBreaks;
            var isBeginOfLine = leadingLineBreaks > 0;
            foreach
                (
                var token in
                    Target.Token.PrecededWith.Where(token => !Lexer.IsWhiteSpace(token)))
            {
                if(Lexer.IsLineEnd(token)
                    && !Formatter.IsRelevantLineBreak(emptyLines, Target.TokenClass))
                    continue;

                if(isBeginOfLine && !Lexer.IsLineEnd(token))
                    result += indent;

                result += token.Characters.Id;

                if(Lexer.IsLineEnd(token))
                    emptyLines++;
                else
                    emptyLines = Lexer.IsLineComment(token) ? 1 : 0;

                isBeginOfLine = !Lexer.IsComment(token);
            }

            if(isBeginOfLine)
                result += indent;

            return result == ""
                ? SeparatorType.Get(LeftTokenClass, Target.TokenClass).Text
                : result;
        }

        [DisableDump]
        int? LineLength
        {
            get
            {
                if(Target == null)
                    return 0;

                var w = LineLengthOfWhitespaces;
                if(w == null)
                    return null;

                var l = Left.LineLength;
                if(l == null)
                    return null;

                var r = Right.LineLength;
                if(r == null)
                    return null;

                return w + l + r + Target.Token.Characters.Id.Length;
            }
        }

        [DisableDump]
        int? LineLengthOfWhitespaces
        {
            get
            {
                var whiteSpaceTokens = Target.Token.PrecededWith;
                if(whiteSpaceTokens.HasLineComment())
                    return null;

                if(
                    whiteSpaceTokens
                        .Any
                        (
                            token =>
                                Lexer.IsLineEnd(token)
                                    && Formatter.IsRelevantLineBreak(0, Target.TokenClass)))
                    return null;

                if(whiteSpaceTokens
                    .Any(token => Lexer.IsComment(token) && token.Characters.Id.Contains("\n")))
                    return null;

                var result = whiteSpaceTokens
                    .Where(Lexer.IsComment)
                    .Sum(token => token.Characters.Id.Length);

                if(result > 0)
                    return result;

                var separatorType = SeparatorType.Get(LeftTokenClass, Target.TokenClass);
                return separatorType == SeparatorType.Contact ? 0 : 1;
            }
        }

        [DisableDump]
        SmartFormat.IRightBraceMode RightBraceMode
        {
            get
            {
                if(Target.TokenClass is LeftParenthesis)
                    return Right?.BraceMode?.RightMode;
                return null;
            }
        }

        [DisableDump]
        SmartFormat.IBraceMode BraceMode
        {
            get
            {
                if(Target?.TokenClass is List)
                    return ListMode?.BraceMode;
                return null;
            }
        }

        [DisableDump]
        SmartFormat.IListMode ListMode => IsListLineMode ? ListLineMode.Instance : null;

        [DisableDump]
        bool IsListLineMode
        {
            get
            {
                if(!(Target?.TokenClass is List))
                    return false;

                if(IsBraced)
                {
                    if(Parent.ReducedWhitespaces.Contains("\n"))
                        return true;
                    if(Parent.Parent.ReducedWhitespaces.Contains("\n"))
                        return true;
                }

                return IsLineBreakRequired;
            }
        }

        [DisableDump]
        bool IsLineBreakRequired
            => Formatter.MaxLineLength == null
                ? HasLineBreak()
                : LineFillCount(Formatter.MaxLineLength.Value) <= 0;

        [DisableDump]
        string ReducedWhitespaces => InternalGetWhitespaces(0, 0);

        [DisableDump]
        IEnumerable<Frame> ParentChain
        {
            get
            {
                if(Parent != null)
                {
                    yield return Parent;
                    foreach(var frame in Parent.ParentChain)
                        yield return frame;
                }
            }
        }

        [DisableDump]
        int IndentLevel => ParentChain.Count(item => item.HasIndent) + (HasIndent ? 1 : 0);

        [DisableDump]
        bool HasIndent
        {
            get
            {
                if(Target.TokenClass is List)
                    return false;

                return RootListFrameOfItem?.IsBraced ?? false;
            }
        }

        [DisableDump]
        Frame RootListFrameOfItem => Target.TokenClass is List ? null : Parent?.RootListFrame;

        [DisableDump]
        Frame RootListFrame
        {
            get
            {
                if(!(Target.TokenClass is List))
                    return null;

                if(Parent?.Target.TokenClass is List)
                    return Parent.RootListFrame;

                return this;
            }
        }

        [DisableDump]
        bool IsBraced
            => Parent != null
                && Target == Parent.Target.Right
                && Parent.IsBraceConstructAtLeftBrace;

        [DisableDump]
        bool IsBraceConstructAtLeftBrace
            => Parent != null
                && Target == Parent.Target.Left
                && Parent.IsBraceConstruct;

        [DisableDump]
        bool IsBraceConstruct
        {
            get
            {
                var rightBrace = Target?.TokenClass as RightParenthesis;
                if(rightBrace == null)
                    return false;

                var leftBraceItem = Left?.Target;
                if(leftBraceItem == null || leftBraceItem != Target.Left)
                    return false;

                var leftBrace = leftBraceItem.TokenClass as LeftParenthesis;
                return leftBrace?.Level == rightBrace.Level;
            }
        }

        [DisableDump]
        int LeadingLineBreaks => _leadingLineBreaksCache.IsBusy ? 0 : _leadingLineBreaksCache.Value;

        internal IEnumerable<Item> GetItems()
        {
            if(Target == null)
                yield break;
            foreach(var item in Left.Items)
                yield return item;
            yield return Item;
            foreach(var item in Right.Items)
                yield return item;
        }

        bool HasLineBreak(bool isReduced = true)
        {
            if(Target == null)
                return false;

            if(Target.Left != null && Left.HasLineBreak(isReduced))
                return true;

            var spaces = (Target.Left == null && isReduced) ? ReducedWhitespaces : Whitespaces;
            if(spaces.Contains("\n"))
                return true;

            return Right.HasLineBreak(false);
        }

        static int _depth;

        int LineFillCount(int maxLineLength, bool isReduced = true)
        {
            Tracer.ConditionalBreak(_depth > 100);
            _depth++;
            try
            {
                if(maxLineLength <= 0)
                    return 0;

                if(Target == null)
                    return maxLineLength;

                var result = maxLineLength;

                if(Target.Left != null)
                    result = Left.LineFillCount(result, isReduced);

                var spaces = (Target.Left == null && isReduced) ? ReducedWhitespaces : Whitespaces;
                if(spaces.Contains("\n"))
                    return 0;

                result -= spaces.Length;
                result -= Target.Token.Characters.Id.Length;

                return Right.LineFillCount(result, false);
            }
            finally
            {
                _depth--;
            }
        }

        [DisableDump]
        [UsedImplicitly]
        internal string Reformat => Items.Combine().Filter();

        internal static Frame CreateFrame(SourceSyntax target, SmartFormat formatter)
        {
            var result = new Frame(target, formatter: formatter);
            var situation = result.GetSituation(Rulers.Empty);
            return result;
        }
    }

    sealed class ListLineMode : DumpableObject, SmartFormat.IListMode
    {
        internal static readonly SmartFormat.IListMode Instance = new ListLineMode();

        SmartFormat.IBraceMode SmartFormat.IListMode.BraceMode => BraceLineMode.Instance;

        SmartFormat.IMode SmartFormat.IListMode.Combine
            (SmartFormat.IMode previous, SmartFormat.IMode current)
            => previous == null && current == null
                ? LineBreakMode.Instance
                : DoubleLineBreakMode.Instance;
    }

    sealed class BraceLineMode : DumpableObject, SmartFormat.IBraceMode
    {
        internal static readonly SmartFormat.IBraceMode Instance = new BraceLineMode();

        SmartFormat.IRightBraceMode SmartFormat.IBraceMode.RightMode => RightBraceLineMode.Instance;
        SmartFormat.IMode SmartFormat.IBraceMode.Mode => LineBreakMode.Instance;
    }

    sealed class RightBraceLineMode : DumpableObject, SmartFormat.IRightBraceMode
    {
        internal static readonly SmartFormat.IRightBraceMode Instance = new RightBraceLineMode();
        SmartFormat.IMode SmartFormat.IRightBraceMode.Mode => LineBreakMode.Instance;
    }

    sealed class LineBreakMode : DumpableObject, SmartFormat.IMode
    {
        internal static readonly SmartFormat.IMode Instance = new LineBreakMode();
        int SmartFormat.IMode.LeadingLineBreaks => 1;
    }

    sealed class DoubleLineBreakMode : DumpableObject, SmartFormat.IMode
    {
        internal static readonly SmartFormat.IMode Instance = new DoubleLineBreakMode();
        int SmartFormat.IMode.LeadingLineBreaks => 2;
    }
}