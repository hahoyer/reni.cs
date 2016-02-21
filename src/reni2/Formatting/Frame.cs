using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using Reni.TokenClasses;

namespace Reni.Formatting
{
    sealed class Frame : DumpableObject
    {
        internal static Frame Create(SourceSyntax target, HierachicalFormatter formatter)
        {
            var result = new Frame(target, formatter: formatter);
            return result;
        }

        readonly ValueCache<Frame> LeftCache;
        readonly ValueCache<Frame> RightCache;
        readonly ValueCache<ResultItems> ItemsWithoutLeadingBreaksCache;
        readonly ValueCache<bool> HasInnerLineBreaksCache;
        readonly ValueCache<int> LeadingLineBreaksCache;

        [DisableDump]
        readonly HierachicalFormatter Formatter;
        [DisableDump]
        readonly Frame Parent;
        [DisableDump]
        internal readonly SourceSyntax Target;
        internal string TargetString => Target.SourcePart.NodeDump;


        Frame(SourceSyntax target, Frame parent = null, HierachicalFormatter formatter = null)
        {
            Parent = parent;
            Formatter = parent?.Formatter ?? formatter;
            Target = target;
            LeftCache = new ValueCache<Frame>(() => new Frame(Target.Left, this));
            RightCache = new ValueCache<Frame>(() => new Frame(Target.Right, this));
            ItemsWithoutLeadingBreaksCache = new ValueCache<ResultItems>
                (GetItemsWithoutLeadingBreaks);
            LeadingLineBreaksCache = new ValueCache<int>(GetLeadingLineBreaksForCache);
            HasInnerLineBreaksCache = new ValueCache<bool>(GetHasInnerLineBreaksForCache);
        }

        protected override string GetNodeDump()
            => base.GetNodeDump() + "(" + Target?.TokenClass.GetType().PrettyName() + ")";

        Frame Left => LeftCache.Value;
        Frame Right => RightCache.Value;
        ResultItems ItemsWithoutLeadingBreaks => ItemsWithoutLeadingBreaksCache.Value;
        bool HasInnerLineBreaks => HasInnerLineBreaksCache.Value;
        int LeadingLineBreaks => LeadingLineBreaksCache.Value;
        [DisableDump]
        internal Frame LeftNeighbor => Left.RightMostTokenClassFrame ?? LeftTokenClassFrame;

        [DisableDump]
        internal int IndentLevel => (Parent?.IndentLevel ?? 0) + (HasIndent ? 1 : 0);

        bool HasIndent
        {
            get
            {
                var tokenClass = Target.TokenClass;
                if(tokenClass is List)
                    return false;

                var parentClass = Parent?.Target.TokenClass;
                if(parentClass is List)
                    return ParentChain.Any(item => item.Target.TokenClass is RightParenthesis);

                if(tokenClass is LeftParenthesis)
                    return false;

                return parentClass is LeftParenthesis || parentClass is RightParenthesis;
            }
        }

        Frame LeftNeighborOfTarget => Parent?.Left == this ? Parent.LeftNeighborOfTarget : Parent;
        bool RequiresLineBreak
            =>
                IsLineBreakRuler
                    ? Formatter.RequiresLineBreak(ItemsWithoutLeadingBreaks.Format())
                    : Parent?.RequiresLineBreak ?? false;

        bool IsLineBreakRuler
            => Target.TokenClass is RightParenthesis
                || Target.TokenClass is EndToken;

        Frame RightMostTokenClassFrame
            => Target == null
                ? null
                : Target.Right == null ? this : Right.RightMostTokenClassFrame;

        Frame LeftTokenClassFrame
            => Parent == null || Target == Parent.Target.Right
                ? Parent
                : Parent.LeftTokenClassFrame;

        bool IsLineBreakCandidate
        {
            get
            {
                var rightToken = Target.TokenClass;

                if(rightToken is List)
                    return false;

                if(rightToken is RightParenthesis
                    || rightToken is LeftParenthesis
                    || rightToken is EndToken
                    )
                    return true;

                var leftToken = LeftNeighbor?.Target.TokenClass;

                return leftToken is List
                    || leftToken is RightParenthesis
                    || leftToken is LeftParenthesis;
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

        int GetLeadingLineBreaksForCache()
        {
            if(!IsLineBreakCandidate)
                return 0;

            if(!LeadingLineBreaksDependsOnLeftSite)
                return RequiresLineBreak ? 1 : 0;

            if(!LeftNeighbor.RequiresLineBreak)
                return 0;

            var leftNeighborOfTarget = LeftNeighborOfTarget;
            if(leftNeighborOfTarget == null)
                return 1;
            return leftNeighborOfTarget.RequiresAdditionalLineBreak ? 2 : 1;
        }

        bool GetHasInnerLineBreaksForCache()
            => ItemsWithoutLeadingBreaksCache.Value.HasInnerLineBreaks();

        ResultItems GetItemsWithoutLeadingBreaks()
        {
            var trace = false;
            StartMethodDump(trace);
            try
            {
                var result = CollectItemsWithoutLeadingBreaks()
                    .Aggregate(new ResultItems(), (c, n) => c.Combine(n));
                return ReturnMethodDump(result);
            }
            finally
            {
                EndMethodDump();
            }
        }

        [DisableDump]
        internal ResultItems ItemsForResult
            => CollectItemsForResult()
                .Aggregate(new ResultItems(), (c, n) => c.Combine(n));

        IEnumerable<ResultItems> CollectItemsForResult()
        {
            if(Target == null)
                yield break;
            yield return Left.ItemsForResult;
            yield return Formatter.Item(this, LeadingLineBreaks);
            yield return Right.ItemsForResult;
        }

        IEnumerable<ResultItems> CollectItemsWithoutLeadingBreaks()
        {
            if(Target == null)
                yield break;
            yield return Left.ItemsWithoutLeadingBreaks;
            yield return Formatter.Item(this);
            yield return Right.ItemsWithoutLeadingBreaks;
        }
    }
}