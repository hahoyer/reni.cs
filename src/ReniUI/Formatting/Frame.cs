using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    sealed class Frame : DumpableObject
    {
        internal static Frame Create(HierachicalFormatter formatter, CompilerBrowser compiler, SourcePart targetPart)
        {
            var result = new Frame(compiler.Locate(targetPart), compiler, formatter: formatter);
            return result;
        }

        [DisableDump]
        readonly HierachicalFormatter Formatter;

        readonly ValueCache<bool> HasInnerLineBreaksCache;
        readonly ValueCache<ResultItems> ItemsWithoutLeadingBreaksCache;
        readonly ValueCache<int> LeadingLineBreaksCache;

        readonly ValueCache<Frame> LeftCache;

        [DisableDump]
        readonly Frame Parent;

        readonly ValueCache<Frame> RightCache;

        [DisableDump]
        internal readonly Syntax Target;

        readonly CompilerBrowser Compiler;

        Frame(Syntax target, CompilerBrowser compiler, Frame parent = null, HierachicalFormatter formatter = null)
        {
            Parent = parent;
            Formatter = parent?.Formatter ?? formatter;
            Target = target;
            Compiler = compiler;
            LeftCache = new ValueCache<Frame>(() => new Frame(Target.Left, Compiler, this));
            RightCache = new ValueCache<Frame>(() => new Frame(Target.Right, Compiler, this));
            ItemsWithoutLeadingBreaksCache = new ValueCache<ResultItems>
                (GetItemsWithoutLeadingBreaks);
            LeadingLineBreaksCache = new ValueCache<int>(GetLeadingLineBreaksForCache);
            HasInnerLineBreaksCache = new ValueCache<bool>(GetHasInnerLineBreaksForCache);
        }

        internal string TargetString => Target.SourcePart.NodeDump;

        Frame Left => LeftCache.Value;
        Frame Right => RightCache.Value;
        ResultItems ItemsWithoutLeadingBreaks => ItemsWithoutLeadingBreaksCache.Value;
        bool HasInnerLineBreaks => HasInnerLineBreaksCache.Value;
        int LeadingLineBreaks => LeadingLineBreaksCache.Value;

        [DisableDump]
        internal Frame LeftNeighbor => Left.RightMostTokenClassFrame ?? LeftTokenClassFrame;

        [DisableDump]
        internal int IndentLevel => (Parent?.IndentLevel ?? -1) + (HasIndent ? 1 : 0);

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
                    ? Formatter.RequiresLineBreak(ItemsWithoutLeadingBreaks.Text)
                    : Parent?.RequiresLineBreak ?? false;

        bool IsLineBreakRuler
            => Target.TokenClass is RightParenthesis;

        Frame RightMostTokenClassFrame
            => Target == null
                ? null
                : Target.Right == null
                    ? this
                    : Right.RightMostTokenClassFrame;

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

                if(rightToken is RightParenthesis || rightToken is LeftParenthesis
                )
                    return true;

                var leftToken = LeftNeighbor?.Target.TokenClass;

                return leftToken is List || leftToken is RightParenthesis || leftToken is LeftParenthesis;
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
                return leftToken is List || leftToken is RightParenthesis || !(rightToken is LeftParenthesis);
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

        [DisableDump]
        internal ResultItems ItemsForResult
            => CollectItemsForResult().Aggregate(ResultItems.Default);

        protected override string GetNodeDump()
            => base.GetNodeDump() + "(" + Target?.TokenClass.GetType().PrettyName() + ")";

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
            => CollectItemsWithoutLeadingBreaks().Aggregate(ResultItems.Default);

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