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
        readonly ValueCache<Frame> LeftCache;
        readonly ValueCache<Frame> RightCache;
        readonly ValueCache<Frame> LeftNeighborCache;
        readonly ValueCache<Frame> LineBreakRulerForCache;

        readonly ValueCache<bool> HasInnerLineBreaksCache;
        readonly ValueCache<int> LeadingLineBreaksCache;
        readonly ValueCache<Situation> SituationOfRootCache;
        readonly ValueCache<Item> ItemCache;
        readonly ValueCache<IEnumerable<Item>> ItemsCache;

        [DisableDump]
        readonly Provider Formatter;
        [DisableDump]
        readonly Frame Parent;
        internal readonly SourceSyntax Target;

        internal Frame(SourceSyntax target, Frame parent = null, Provider formatter = null)
        {
            Parent = parent;
            Formatter = parent?.Formatter ?? formatter;
            Target = target;
            LeftCache = new ValueCache<Frame>(() => new Frame(Target.Left, this));
            RightCache = new ValueCache<Frame>(() => new Frame(Target.Right, this));
            LeftNeighborCache = new ValueCache<Frame>
                (() => Left.RightMostTokenClassFrame ?? LeftTokenClassFrame);
            ItemsCache = new ValueCache<IEnumerable<Item>>(GetItems);
            ItemCache = new ValueCache<Item>(() => new Item(Whitespaces, Target.Token));
            LeadingLineBreaksCache = new ValueCache<int>(GetLeadingLineBreaksForCache);
            SituationOfRootCache = new ValueCache<Situation>(GetSituationOfRootForCache);
            LineBreakRulerForCache = new ValueCache<Frame>(GetLineBreakRulerForCache);
            HasInnerLineBreaksCache = new ValueCache<bool>(() => HasLineBreaks(true));
        }

        protected override string GetNodeDump()
            => base.GetNodeDump() + "(" + Target?.TokenClass.GetType().PrettyName() + ")";

        [DisableDump]
        Frame Left => LeftCache.Value;

        [DisableDump]
        Frame Right => RightCache.Value;

        [DisableDump]
        Item Item => ItemCache.Value;

        [DisableDump]
        IEnumerable<Item> Items => ItemsCache.Value;

        [DisableDump]
        Frame LeftNeighbor => LeftNeighborCache.Value;

        [DisableDump]
        string Whitespaces => Formatter.InternalGetWhitespaces
            (
                ()=>LeftTokenClass,
                LeadingLineBreaks,
                IndentLevel,
                Target.LeadingWhiteSpaceTokens,
                Target.TokenClass
            );

        [DisableDump]
        ITokenClass LeftTokenClass => LeftNeighbor?.Target.TokenClass;

        [DisableDump]
        Frame RightMostTokenClassFrame
            => Target == null
                ? null
                : Target.Right == null ? this : Right.RightMostTokenClassFrame;

        [DisableDump]
        Frame LeftTokenClassFrame
            => Parent == null || Target == Parent.Target.Right
                ? Parent
                : Parent.LeftTokenClassFrame;

        [DisableDump]
        int LeadingLineBreaks => LeadingLineBreaksCache.Value;

        Situation SituationOfRoot => SituationOfRootCache.Value;

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

        [DisableDump]
        Frame LineBreakRuler => LineBreakRulerForCache.Value;

        [DisableDump]
        bool IsLineBreakRuler
            => Target.TokenClass is EndToken || Target.TokenClass is RightParenthesis;

        [DisableDump]
        bool HasInnerLineBreaks => HasInnerLineBreaksCache.Value;

        bool IsMultiLineSituation => SituationOfRoot.Rulers.IsMultiLine(LineBreakRuler.Target);

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

        bool HasLineBreaks(bool ignoreLeadingLineBreak)
        {
            if(Target == null)
                return false;

            return Target.Left != null && Left.HasLineBreaks(ignoreLeadingLineBreak)
                || (Target.Left != null || !ignoreLeadingLineBreak) && LeadingLineBreaks > 0
                || Formatter.InternalGetWhitespaces
                    (
                        ()=>LeftTokenClass,
                        0,
                        0,
                        Target.LeadingWhiteSpaceTokens,
                        Target.TokenClass
                    ).Contains("\n")
                || Target.Right != null && Right.HasLineBreaks(false);
        }

        Situation GetSituation(Rulers rulers)
        {
            if(Target == null)
                return Situation.Empty;

            if(IsLineBreakRuler)
                return GetRulerSituation(rulers, true)
                    .Combine(Target, GetRulerSituation(rulers, false), Formatter);

            return PairSituation(rulers);
        }

        Situation PairSituation(Rulers rulers)
        {
            var left = Left.GetSituation(rulers);
            var token = GetTokenSituation(rulers);
            var right = Right.GetSituation(rulers);
            Tracer.Assert(rulers.Data.Any(item => item.Value) || token.LineCount == 0);
            return left + (token + right);
        }

        Situation GetRulerSituation(Rulers rulers, bool isMultiLine)
            => PairSituation(rulers.Concat(Target, isMultiLine));

        Situation GetTokenSituation(Rulers rulers)
        {
            var lengths = GetWhitespaceSituationString(rulers)
                .Split('\n')
                .Select(item => item.Length);
            return Situation
                .Create(lengths)
                + Target.Token.Characters.Length;
        }

        bool HasLeadingLineBreaks(Rulers rulers)
            => !HasNeverLeadingLineBreaks
                && rulers.IsMultiLine
                    (
                        (LeadingLineBreaksDependsOnLeftSite ? LeftNeighbor : this)
                            .LineBreakRuler
                            .Target
                    );

        string GetWhitespaceSituationString(Rulers rulers)
            => Formatter.InternalGetWhitespaces
                (
                    ()=>LeftTokenClass,
                    HasLeadingLineBreaks(rulers) ? 1 : 0,
                    0,
                    Target.LeadingWhiteSpaceTokens,
                    Target.TokenClass
                );

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

        Frame GetLineBreakRulerForCache() => IsLineBreakRuler ? this : Parent?.LineBreakRuler;

        Situation GetSituationOfRootForCache()
            => Parent == null ? GetSituation(Rulers.Empty) : Parent.SituationOfRoot;

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

        internal static Frame CreateFrame(SourceSyntax target, Provider formatter)
        {
            var frame = new Frame(target, formatter: formatter);
            return frame;
        }
    }
}