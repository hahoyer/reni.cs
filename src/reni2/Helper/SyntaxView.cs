using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using Reni.Parser;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.Helper
{
    public abstract class SyntaxView<TTarget> : DumpableObject, ValueCache.IContainer, ITree<TTarget>
        where TTarget : SyntaxView<TTarget>
    {
        class CacheContainer
        {
            public FunctionCache<int, TTarget> DirectChildren;
            public FunctionCache<SourcePosition, (TTarget, int)> LocateByPosition;
            public FunctionCache<SourcePosition, (TTarget, int)> LocateByPositionIncludingParent;
        }

        internal readonly PositionDictionary<TTarget> Context;

        internal readonly Syntax FlatItem;
        internal readonly TTarget Parent;
        readonly CacheContainer Cache = new CacheContainer();
        readonly int Index;

        protected SyntaxView(Syntax flatItem, TTarget parent, PositionDictionary<TTarget> context, int index)
        {
            flatItem.AssertIsNotNull();
            FlatItem = flatItem;
            Parent = parent;
            Context = context;
            Index = index;

            foreach(var anchor in FlatItem.Anchor.Items)
                Context[anchor] = (TTarget)this;

            Cache.LocateByPosition =
                new FunctionCache<SourcePosition, (TTarget, int)>(i => LocateByPositionForCache(i, false));
            Cache.LocateByPositionIncludingParent
                = new FunctionCache<SourcePosition, (TTarget, int)>(i => LocateByPositionForCache(i, true));
            Tracer.ConditionalBreak(flatItem.ObjectId == -492);
        }

        [DisableDump]
        public SourcePart[] Anchors => FlatItem.Anchor.SourceParts;

        [DisableDump]
        public SourcePart SourcePart => FlatItem.Anchor.SourcePart;

        [DisableDump]
        internal TTarget LeftMost => this.GetNodesFromLeftToRight().First();

        [DisableDump]
        internal TTarget RightMost => this.GetNodesFromRightToLeft().First();


        int LeftDirectChildCount => FlatItem.LeftDirectChildCount;
        int DirectChildCount => FlatItem.DirectChildren.Length;

        internal TTarget[] DirectChildren
            => this.CachedValue(() => DirectChildCount.Select(GetDirectChild).ToArray());

        [DisableDump]
        internal TTarget LeftNeighbor => RightMostLeftSibling?.RightMost ?? LeftParent;

        [DisableDump]
        internal TTarget RightNeighbor => LeftMostRightSibling?.LeftMost ?? RightParent;

        [DisableDump]
        internal TTarget RightMostLeftSibling => DirectChildren[LeftDirectChildCount - 1];


        [DisableDump]
        internal TTarget LeftMostRightSibling => DirectChildren[LeftDirectChildCount];

        [DisableDump]
        TTarget LeftParent
            => Parent != null && Parent.LeftChildren.Any(node => node == this)
                ? Parent.LeftParent
                : Parent;

        [DisableDump]
        TTarget RightParent
            => Parent != null && Parent.RightChildren.Any(node => node == this)
                ? Parent.RightParent
                : Parent;

        [DisableDump]
        TTarget[] LeftChildren
            => this.CachedValue(() => LeftDirectChildCount.Select(index => DirectChildren[index]).ToArray());

        [DisableDump]
        TTarget[] RightChildren => this.CachedValue(GetRightChildren);

        [DisableDump]
        internal bool IsLeftChild => Parent?.RightMostLeftSibling == this;

        [DisableDump]
        internal bool IsRightChild => Parent?.LeftMostRightSibling == this;

        ValueCache ValueCache.IContainer.Cache { get; } = new ValueCache();
        int ITree<TTarget>.DirectChildCount => DirectChildCount;
        TTarget ITree<TTarget>.GetDirectChild(int index) => DirectChildren[index];

        int ITree<TTarget>.LeftDirectChildCount => LeftDirectChildCount;

        TTarget GetDirectChild(int index)
        {
            var child = FlatItem.DirectChildren[index];
            return child == null? null : Create(child, index);
        }

        protected abstract TTarget Create(Syntax syntax, int index);

        (TTarget Master, int Index) LocateByPositionForCache(SourcePosition current, bool includingParent)
        {
            (!includingParent).Assert();
            return Context[current]
                .Single(node => node.Length > 0)
                .Node
                .LocateByPosition(current);
        }

        (TTarget Master, int Index) LocateByPosition(SourcePosition current)
        {
            var result = Anchors
                .Select((anchor, index) => (anchor, index))
                .FirstOrDefault(node => node.anchor.Length > 0 && node.anchor.Contains(current));

            if(result.anchor != null)
                return ((TTarget)this, result.index);

            NotImplementedMethod(current);
            return default;
        }

        internal(TTarget Master, int Index) LocateByPosition(SourcePosition offset, bool includingParent)
            => includingParent? Cache.LocateByPositionIncludingParent[offset] : Cache.LocateByPosition[offset];

        internal IEnumerable<int> GetParserLevelGroup(int index)
        {
            var target = FlatItem.Anchor.Items[index];
            return FlatItem
                .Anchor
                .Items
                .Select((n, i) => (n, i))
                .Where(item => target.TokenClass.IsBelongingTo(item.n.TokenClass))
                .Select(item => item.i);
        }

        TTarget[] GetRightChildren()
            => (DirectChildCount - LeftDirectChildCount)
                .Select(index => DirectChildren[index + LeftDirectChildCount])
                .ToArray();

        TContainer FlatSubFormat<TContainer, TValue>(BinaryTree left, bool areEmptyLinesPossible)
            where TContainer : class, IFormatResult<TValue>, new()
            => left == null? new TContainer() : FlatFormat<TContainer, TValue>(left, areEmptyLinesPossible);

        TContainer FlatFormat<TContainer, TValue>(BinaryTree target, bool areEmptyLinesPossible)
            where TContainer : class, IFormatResult<TValue>, new()
        {
            var tokenString = target.Token.Characters
                .FlatFormat(target.Left == null? null : target.Token.PrecededWith, areEmptyLinesPossible);

            if(tokenString == null)
                return null;

            tokenString = (GetIsSeparatorRequired(target)? " " : "") + tokenString;

            var leftResult = FlatSubFormat<TContainer, TValue>(target.Left, areEmptyLinesPossible);
            if(leftResult == null)
                return null;

            var rightResult = FlatSubFormat<TContainer, TValue>(target.Right, areEmptyLinesPossible);
            return rightResult == null? null : leftResult.Concat(tokenString, rightResult);
        }

        IEnumerable<TResult> FlatFormat<TContainer, TResult>(bool areEmptyLinesPossible)
            where TContainer : class, IFormatResult<TResult>, new()
        {
            var results = FlatItem
                .Anchor
                .Items
                .Select(item => FlatFormat<TContainer, TResult>(item, areEmptyLinesPossible));

            return results.Any(item => item == null)
                ? null 
                : results.Select(item=>item.Value);
        }

        /// <summary>
        ///     Try to format target into one line.
        /// </summary>
        /// <param name="areEmptyLinesPossible"></param>
        /// <returns>The formatted line or null if target contains line breaks.</returns>
        internal string FlatFormat(bool areEmptyLinesPossible) 
            => FlatFormat<StringResult, string>(areEmptyLinesPossible)
                ?.Stringify("");

        /// <summary>
        ///     Get the line length of target when formatted as one line.
        /// </summary>
        /// <param name="areEmptyLinesPossible"></param>
        /// <returns>The line length calculated or null if target contains line breaks.</returns>
        internal int? GetFlatLength(bool areEmptyLinesPossible) 
            => FlatFormat<IntegerResult, int>(areEmptyLinesPossible)
                ?.Sum();

        bool GetIsSeparatorRequired(BinaryTree current)
            => !current.Token.PrecededWith.HasComment() &&
               SeparatorExtension.Get(GetLeftNeighbor(current)?.TokenClass, current.TokenClass);

        internal BinaryTree GetLeftNeighbor(BinaryTree current)
        {
            NotImplementedMethod(current);
            return default;
        }

    }
}