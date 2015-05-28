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

        readonly SmartFormat _formatter;
        readonly Frame _parent;
        readonly SourceSyntax _target;

        internal Frame(SourceSyntax target, Frame parent = null, SmartFormat formatter = null)
        {
            _parent = parent;
            _formatter = parent?._formatter ?? formatter;
            _target = target;
            _leftCache = new ValueCache<Frame>(() => new Frame(_target.Left, this));
            _rightCache = new ValueCache<Frame>(() => new Frame(_target.Right, this));
            _leftNeighborCache = new ValueCache<Frame>(() => Left.RightMostTokenClassFrame ?? LeftTokenClassFrame);
            _itemsCache = new ValueCache<IEnumerable<Item>>(GetItems);
            _itemCache = new ValueCache<Item>(() => new Item(Whitespaces, _target.Token));
            _leadingLineBreaksCache = new ValueCache<int>(GetLeadingLineBreaksForCache);
        }

        protected override string GetNodeDump()
            => base.GetNodeDump() + "(" + _target?.TokenClass.GetType().PrettyName() + ")";

        [DisableDump]
        Frame Left => _leftCache.Value;

        [DisableDump]
        Frame Right => _rightCache.Value;

        [DisableDump]
        Item Item => _itemCache.Value;

        [DisableDump]
        IEnumerable<Item> Items => _itemsCache.Value;

        Frame LeftNeighbor => _leftNeighborCache.Value;

        string Whitespaces => InternalGetWhitespaces(LeadingLineBreaks, IndentLevel, true);

        ITokenClass LeftTokenClass => LeftNeighbor?._target.TokenClass;

        Frame RightMostTokenClassFrame
            => _target == null ? null : _target.Right == null ? this : Right.RightMostTokenClassFrame;

        Frame LeftTokenClassFrame
            => _parent == null || _target == _parent._target.Right
                ? _parent
                : _parent.LeftTokenClassFrame;

        bool FormatsToMultiline
        {
            get
            {
                var ll = LineLength;
                return ll == null || ll.Value >= _formatter.MaxLineLength;
            }
        }

        string InternalGetWhitespaces(int leadingLineBreaks, int indentLevel, bool treatLineBreaks)
        {
            var indent = " ".Repeat(indentLevel * 4);
            var result = "\n".Repeat(leadingLineBreaks);
            var emptyLines = leadingLineBreaks;
            var isBeginOfLine = leadingLineBreaks > 0;
            foreach
                (
                var token in
                    _target.Token.PrecededWith.Where(token => !Lexer.IsWhiteSpace(token)))
            {
                if(Lexer.IsLineEnd(token)
                    && !(treatLineBreaks
                        && _formatter.IsRelevantLineBreak(emptyLines, _target.TokenClass)))
                    continue;
                treatLineBreaks = true;

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
                ? SeparatorType.Get(LeftTokenClass, _target.TokenClass).Text
                : result;
        }

        int GetLeadingLineBreaksForCache()
        {
            if (_target.TokenClass is List)
                return 0;

            if (_target.TokenClass is RightParenthesis)
                return Left?.RightBraceMode?.Mode.LeadingLineBreaks ?? 0;

            var leftNeighbor = LeftNeighbor;

            if (leftNeighbor?._target.TokenClass is List)
            {
                var listMode = leftNeighbor.RootListFrame.ListMode;
                if (listMode == null)
                    return 0;

                if (leftNeighbor.Left.FormatsToMultiline)
                    return 2;

                var current = leftNeighbor.Right;
                if (current._target?.TokenClass is List)
                    current = current.Left;
                return current.FormatsToMultiline ? 2 : 1;
            }

            if (leftNeighbor?._target.TokenClass is RightParenthesis)
                return leftNeighbor.Left?.RightBraceMode?.Mode.LeadingLineBreaks ?? 0;

            if (_target.TokenClass is LeftParenthesis)
                return Right?.BraceMode?.Mode.LeadingLineBreaks ?? 0;

            if(leftNeighbor?._target.TokenClass is LeftParenthesis)
                return leftNeighbor.Right?.BraceMode?.Mode.LeadingLineBreaks ?? 0;

            return 0;
        }

        int? LineLength
        {
            get
            {
                if(_target == null)
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

                return w + l + r + _target.Token.Characters.Id.Length;
            }
        }

        int? LineLengthOfWhitespaces
        {
            get
            {
                var whiteSpaceTokens = _target.Token.PrecededWith;
                if(whiteSpaceTokens.HasLineComment())
                    return null;

                if(
                    whiteSpaceTokens
                        .Any
                        (
                            token =>
                                Lexer.IsLineEnd(token)
                                    && _formatter.IsRelevantLineBreak(0, _target.TokenClass)))
                    return null;

                if(whiteSpaceTokens
                    .Any(token => Lexer.IsComment(token) && token.Characters.Id.Contains("\n")))
                    return null;

                var result = whiteSpaceTokens
                    .Where(Lexer.IsComment)
                    .Sum(token => token.Characters.Id.Length);

                if(result > 0)
                    return result;

                var separatorType = SeparatorType.Get(LeftTokenClass, _target.TokenClass);
                return separatorType == SeparatorType.Contact ? 0 : 1;
            }
        }

        SmartFormat.IRightBraceMode RightBraceMode
        {
            get
            {
                if(_target.TokenClass is LeftParenthesis)
                    return Right?.BraceMode?.RightMode;
                return null;
            }
        }

        SmartFormat.IBraceMode BraceMode
        {
            get
            {
                if(_target?.TokenClass is List)
                    return ListMode?.BraceMode;
                return null;
            }
        }

        SmartFormat.IListMode ListMode => IsListLineMode ? ListLineMode.Instance : null;

        bool IsListLineMode
        {
            get
            {
                if(!(_target?.TokenClass is List))
                    return false;

                if(IsBraced)
                {
                    if(_parent.ReducedWhitespaces.Contains("\n"))
                        return true;
                    if(_parent._parent.ReducedWhitespaces.Contains("\n"))
                        return true;
                }

                return IsLineBreakRequired;
            }
        }

        bool IsLineBreakRequired
            => _formatter.MaxLineLength == null
                ? HasLineBreak()
                : LineFillCount(_formatter.MaxLineLength.Value) <= 0;

        string ReducedWhitespaces => InternalGetWhitespaces(0, IndentLevel, false);


        IEnumerable<Frame> ParentChain
        {
            get
            {
                if(_parent != null)
                {
                    yield return _parent;
                    foreach(var frame in _parent.ParentChain)
                        yield return frame;
                }
            }
        }

        int IndentLevel => ParentChain.Count(item => item.HasIndent) + (HasIndent ? 1 : 0);

        bool HasIndent
        {
            get
            {
                if(_target.TokenClass is List)
                    return false;

                return RootListFrameOfItem?.IsBraced ?? false;
            }
        }

        Frame RootListFrameOfItem => _target.TokenClass is List ? null : _parent?.RootListFrame;

        Frame RootListFrame
        {
            get
            {
                if(!(_target.TokenClass is List))
                    return null;

                if(_parent?._target.TokenClass is List)
                    return _parent.RootListFrame;

                return this;
            }
        }

        bool IsBraced
            => _parent != null
                && _target == _parent._target.Right
                && _parent.IsBraceConstructAtLeftBrace;

        bool IsBraceConstructAtLeftBrace
            => _parent != null
                && _target == _parent._target.Left
                && _parent.IsBraceConstruct;

        bool IsBraceConstruct
        {
            get
            {
                var rightBrace = _target?.TokenClass as RightParenthesis;
                if(rightBrace == null)
                    return false;

                var leftBraceItem = Left?._target;
                if(leftBraceItem == null || leftBraceItem != _target.Left)
                    return false;

                var leftBrace = leftBraceItem.TokenClass as LeftParenthesis;
                return leftBrace?.Level == rightBrace.Level;
            }
        }

        int LeadingLineBreaks => _leadingLineBreaksCache.IsBusy ? 0 : _leadingLineBreaksCache.Value;

        internal IEnumerable<Item> GetItems()
        {
            if(_target == null)
                yield break;
            foreach(var item in Left.Items)
                yield return item;
            yield return Item;
            foreach(var item in Right.Items)
                yield return item;
        }

        bool HasLineBreak(bool isReduced = true)
        {
            if(_target == null)
                return false;

            if(_target.Left != null && Left.HasLineBreak(isReduced))
                return true;

            var spaces = (_target.Left == null && isReduced) ? ReducedWhitespaces : Whitespaces;
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

                if(_target == null)
                    return maxLineLength;

                var result = maxLineLength;

                if(_target.Left != null)
                    result = Left.LineFillCount(result, isReduced);

                var spaces = (_target.Left == null && isReduced) ? ReducedWhitespaces : Whitespaces;
                if(spaces.Contains("\n"))
                    return 0;

                result -= spaces.Length;
                result -= _target.Token.Characters.Id.Length;

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