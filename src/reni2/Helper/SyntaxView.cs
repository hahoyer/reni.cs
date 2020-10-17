using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using Reni.Parser;

namespace Reni.Helper
{
    sealed class SyntaxView<TResult> : DumpableObject, ValueCache.IContainer, ITree<TResult>
        where TResult : PairView<TResult>
    {
        internal TResult Master { get; }
        internal readonly Syntax FlatItem;
        readonly TResult[] DirectChildren;
        readonly TResult Parent;

        protected override string GetNodeDump() => base.GetNodeDump() + $"({FlatItem.Anchor?.TokenClass.Id})";

        public SyntaxView(Syntax flatItem, TResult[] directChildren, TResult parent, TResult master)
        {
            Master = master;
            FlatItem = flatItem;
            DirectChildren = directChildren;
            Parent = parent;
        }

        [DisableDump]
        TResult[] LeftChildren
            => this.CachedValue(() => LeftDirectChildCount.Select(index => DirectChildren[index]).ToArray());

        [DisableDump]
        TResult[] RightChildren => this.CachedValue(GetRightChildren);

        int DirectChildCount => FlatItem.DirectChildren.Length;

        [DisableDump]
        internal TResult LeftMost => Master.GetNodesFromLeftToRight(node=>node.Syntax).First();

        [DisableDump]
        internal TResult RightMost => Master.GetNodesFromRightToLeft(node=>node.Syntax).First();

        [DisableDump]
        internal TResult LeftNeighbor => RightMostLeftSibling?.Syntax.RightMost ?? LeftParent;

        [DisableDump]
        internal TResult RightNeighbor => LeftMostRightSibling?.Syntax.LeftMost ?? RightParent;

        [DisableDump]
        internal TResult RightMostLeftSibling => DirectChildren[LeftDirectChildCount - 1];

        int LeftDirectChildCount => FlatItem.LeftDirectChildCount;

        [DisableDump]
        internal TResult LeftMostRightSibling => DirectChildren[LeftDirectChildCount];

        [DisableDump]
        internal bool IsLeftChild => Parent?.Syntax.RightMostLeftSibling.Syntax == this;

        [DisableDump]
        internal bool IsRightChild => Parent?.Syntax.LeftMostRightSibling.Syntax == this;

        [DisableDump]
        TResult LeftParent
            => Parent != null && Parent.Syntax.LeftChildren.Any(node => node.Syntax == this)
                ? Parent.Syntax.LeftParent
                : Parent;

        [DisableDump]
        TResult RightParent
            => Parent != null && Parent.Syntax.RightChildren.Any(node => node.Syntax == this)
                ? Parent.Syntax.RightParent
                : Parent;

        ValueCache ValueCache.IContainer.Cache { get; } = new ValueCache();
        int ITree<TResult>.DirectChildCount => DirectChildCount;

        TResult ITree<TResult>.GetDirectChild(int index) => DirectChildren[index];
        int ITree<TResult>.LeftDirectChildCount => LeftDirectChildCount;

        TResult[] GetRightChildren()
            => (DirectChildCount - LeftDirectChildCount)
                .Select(index => DirectChildren[index + LeftDirectChildCount])
                .ToArray();

    }
}