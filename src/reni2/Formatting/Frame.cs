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
        readonly ValueCache<Situation> _situationOfRootCache;

        [DisableDump]
        internal readonly SmartFormat Formatter;
        [DisableDump]
        readonly Frame _parent;
        internal readonly SourceSyntax Target;

        internal Frame(SourceSyntax target, Frame parent = null, SmartFormat formatter = null)
        {
            _parent = parent;
            Formatter = parent?.Formatter ?? formatter;
            Target = target;
            _leftCache = new ValueCache<Frame>(() => new Frame(Target.Left, this));
            _rightCache = new ValueCache<Frame>(() => new Frame(Target.Right, this));
            _leftNeighborCache = new ValueCache<Frame>
                (() => Left.RightMostTokenClassFrame ?? LeftTokenClassFrame);
            _itemsCache = new ValueCache<IEnumerable<Item>>(GetItems);
            _itemCache = new ValueCache<Item>(() => new Item(Whitespaces, Target.Token));
            _leadingLineBreaksCache = new ValueCache<int>(GetLeadingLineBreaksForCache);
            _situationOfRootCache = new ValueCache<Situation>
                (() => _parent == null ? GetSituation(Rulers.Empty) : _parent.SituationOfRoot);
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
            => _parent == null || Target == _parent.Target.Right
                ? _parent
                : _parent.LeftTokenClassFrame;

        [DisableDump]
        int LeadingLineBreaks => _leadingLineBreaksCache.Value;

        bool HasLineBreaks(bool ignoreLeadingLineBreak)
        {
            if(Target == null)
                return false;

            if(Target.Left != null)
                if(Left.HasLineBreaks(ignoreLeadingLineBreak) || LeadingLineBreaks > 0)
                    return true;

            if(InternalGetWhitespaces(0, 0).Contains("\n"))
                return true;

            if(Target.Right != null)
                return Right.HasLineBreaks(false);

            return false;
        }

        Situation GetSituation(Rulers rulers)
        {
            if(Target == null)
                return Situation.Empty;

            if(IsLineBreakRuler)
                return GetRulerSituation(rulers, true)
                    .Combine(this, GetRulerSituation(rulers, false));

            return PairSituation(rulers);
        }

        Situation PairSituation(Rulers rulers)
        {
            var left = Left.GetSituation(rulers);
            var token = GetTokenSituation(rulers);
            var right = Right.GetSituation(rulers);
            return left + (token + right);
        }

        Situation GetRulerSituation(Rulers rulers, bool isMultiLine)
            => PairSituation(rulers.Concat(this, isMultiLine));

        Situation SituationOfRoot => _situationOfRootCache.Value;

        Situation GetTokenSituation(Rulers rulers)
        {
            var lengths = GetWhitespaceSituationString(rulers)
                .Split('\n')
                .Select(item => item.Length);
            return Situation
                .Create(lengths)
                + Target.Token.Characters.Length;
        }

        bool HasNeverLeadingLineBreaks
        {
            get
            {
                var rightToken = Target.TokenClass;

                if(rightToken is List)
                    return true;

                if(rightToken is RightParenthesis)
                    return false;

                var leftToken = LeftNeighbor?.Target.TokenClass;

                return !(leftToken is List) && !(leftToken is RightParenthesis)
                    && !(rightToken is LeftParenthesis) && !(leftToken is LeftParenthesis);
            }
        }

        bool LeadingLineBreaksDependsOnLeftSite
        {
            get
            {
                var rightToken = Target.TokenClass;
                if(rightToken is RightParenthesis)
                    return false;

                var leftToken = LeftNeighbor?.Target.TokenClass;
                return leftToken is List || leftToken is RightParenthesis
                    || !(rightToken is LeftParenthesis);
            }
        }

        bool HasLeadingLineBreaks(Rulers rulers)
            => !HasNeverLeadingLineBreaks
                && rulers.IsMultiLine
                    (
                        (LeadingLineBreaksDependsOnLeftSite ? LeftNeighbor : this)
                            .LineBreakRuler
                    );


        string GetWhitespaceSituationString(Rulers rulers)
            => InternalGetWhitespaces(HasLeadingLineBreaks(rulers) ? 1 : 0, 0);

        [DisableDump]
        Frame LineBreakRuler => IsLineBreakRuler ? this : _parent?.LineBreakRuler;

        [DisableDump]
        bool IsLineBreakRuler
            => Target.TokenClass is EndToken || Target.TokenClass is RightParenthesis;

        [DisableDump]
        bool HasInnerLineBreaks => HasLineBreaks(true);

        bool IsMultiLineSituation => SituationOfRoot.Rulers.IsMultiLine(LineBreakRuler);

        int GetLeadingLineBreaksForCache()
        {
            if(HasNeverLeadingLineBreaks)
                return 0;

            if(!LeadingLineBreaksDependsOnLeftSite)
                return IsMultiLineSituation ? 1 : 0;

            if(!LeftNeighbor.IsMultiLineSituation)
                return 0;

            return LeftNeighbor.RequiresAdditionalLineBreak ? 2 : 1;
        }

        bool RequiresAdditionalLineBreak
        {
            get
            {
                if(!(Target.TokenClass is List))
                    return false;

                if(Left.HasInnerLineBreaks)
                    return true;

                var current = Right;
                if(current.Target?.TokenClass is List)
                    current = current.Left;
                return current.HasInnerLineBreaks;
            }
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
        Frame RootListFrameOfItem => Target.TokenClass is List ? null : _parent?.RootListFrame;

        [DisableDump]
        Frame RootListFrame
        {
            get
            {
                if(!(Target.TokenClass is List))
                    return null;

                if(_parent?.Target.TokenClass is List)
                    return _parent.RootListFrame;

                return this;
            }
        }

        [DisableDump]
        bool IsBraced
            => _parent != null
                && Target == _parent.Target.Right
                && _parent.IsBraceConstructAtLeftBrace;

        [DisableDump]
        bool IsBraceConstructAtLeftBrace
            => _parent != null
                && Target == _parent.Target.Left
                && _parent.IsBraceConstruct;

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

        [DisableDump]
        [UsedImplicitly]
        internal string Reformat => Items.Combine().Filter();

        internal static Frame CreateFrame(SourceSyntax target, SmartFormat formatter)
            => new Frame(target, formatter: formatter);
    }
}